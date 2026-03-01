import os
import asyncio
from typing import TypedDict, Annotated, Literal
from dotenv import load_dotenv
from langchain.agents import create_react_agent, AgentExecutor
from langchain_mcp_adapters import MultiServerMCPClient
from langchain_openai import ChatOpenAI
from langchain.prompts import PromptTemplate
from langchain_core.messages import HumanMessage
from langgraph.graph import StateGraph, START
from langgraph.graph.message import add_messages
from langgraph.types import Command

load_dotenv()


# 1. Define shared state
class State(TypedDict):
    messages: Annotated[list, add_messages]


# 2. Define Node Functions for each AgentExecutor
def researcher_node(state: State) -> Command[Literal["writer", "__end__"]]:
    """Research node that hands off to writer."""
    # Get the researcher agent executor from the closure
    response = researcher_exec.invoke({"messages": state["messages"]})
    # Native handoff: explicitly tell the graph to move to 'writer'
    return Command(
        update={"messages": [response["output"]]},
        goto="writer"
    )


def writer_node(state: State) -> Command[Literal["editor", "__end__"]]:
    """Writer node that hands off to editor."""
    response = writer_exec.invoke({"messages": state["messages"]})
    # Native handoff: explicitly tell the graph to move to 'editor'
    return Command(
        update={"messages": [response["output"]]},
        goto="editor"
    )


def editor_node(state: State) -> Command[Literal["writer", "__end__"]]:
    """Editor node that can hand back to writer or end."""
    response = editor_exec.invoke({"messages": state["messages"]})
    # Example logic: if editor finds an error, hand back to writer
    if "REVISE" in str(response["output"]):
        return Command(
            update={"messages": [response["output"]]},
            goto="writer"
        )
    return Command(
        update={"messages": [response["output"]]},
        goto="__end__"
    )


# Global variables for agent executors (will be set in main)
researcher_exec = None
writer_exec = None
editor_exec = None


async def main():
    """Run the multi-agent content creation workflow."""
    global researcher_exec, writer_exec, editor_exec
    
    # Check for required API keys
    if not os.getenv("GITHUB_TOKEN"):
        print("Error: GITHUB_TOKEN not found.")
        print("Add GITHUB_TOKEN=your-token to a .env file")
        return
    
    if not os.getenv("TAVILY_API_KEY"):
        print("Error: TAVILY_API_KEY not found.")
        print("Add TAVILY_API_KEY=your-key to a .env file")
        print("Get your API key from: https://app.tavily.com/")
        return
    
    # Initialize LLM
    llm = ChatOpenAI(
        model="gpt-4o-mini",
        temperature=0.7,
        base_url="https://models.inference.ai.azure.com",
        api_key=os.getenv("GITHUB_TOKEN")
    )

    # Load prompts from your local filesystem
    researcher_prompt = PromptTemplate.from_file("templates/researcher.json")
    writer_prompt = PromptTemplate.from_file("templates/writer.json")
    editor_prompt = PromptTemplate.from_file("templates/editor.json")
    
    # Get Tavily API key from environment
    tavily_api_key = os.getenv("TAVILY_API_KEY")
    
    async with MultiServerMCPClient({
        "tavily": {
            "transport": "https",
            "url": f"https://mcp.tavily.com/mcp/?tavilyApiKey={tavily_api_key}",
        }
    }) as client:    
        # Get all tools from MCP servers
        tools = await client.get_tools()
        print(f"Available tools: {[tool.name for tool in tools]}")

        # Create AgentExecutors using create_react_agent
        researcher_agent = create_react_agent(llm, tools, researcher_prompt)
        researcher_exec = AgentExecutor(agent=researcher_agent, tools=tools, verbose=True)
        
        writer_agent = create_react_agent(llm, [], writer_prompt)
        writer_exec = AgentExecutor(agent=writer_agent, tools=[], verbose=True)
        
        editor_agent = create_react_agent(llm, [], editor_prompt)
        editor_exec = AgentExecutor(agent=editor_agent, tools=[], verbose=True)
        
        # 3. Build the Graph without manual edges (Edgeless Handoff)
        builder = StateGraph(State)
        builder.add_node("researcher", researcher_node)
        builder.add_node("writer", writer_node)
        builder.add_node("editor", editor_node)
        
        # Only need to set the entry point
        builder.add_edge(START, "researcher")
        graph = builder.compile()
        
        # Run the workflow
        print("\n" + "="*50)
        print("Starting Multi-Agent Content Creation Workflow")
        print("="*50 + "\n")
        
        initial_message = HumanMessage(content="Research the latest trends in AI and write an article about it.")
        result = await graph.ainvoke({"messages": [initial_message]})
        
        print("\n" + "="*50)
        print("Workflow Complete")
        print("="*50 + "\n")
        print("Final Output:")
        print(result["messages"][-1].content if result["messages"] else "No output")


if __name__ == "__main__":
    asyncio.run(main())
