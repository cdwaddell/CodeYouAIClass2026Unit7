import { ChatOpenAI } from "@langchain/openai";
import { HumanMessage } from "@langchain/core/messages";
import { StateGraph, START, Annotation } from "@langchain/langgraph";
import { createAgent } from "langchain";
import { Command } from "@langchain/langgraph";
import { MultiServerMCPClient } from "@langchain/mcp-adapters";
import dotenv from "dotenv";
import { readFileSync } from "fs";
import readline from "readline";

// Load environment variables
dotenv.config();

// 1. Define shared state annotation
const StateAnnotation = Annotation.Root({
  messages: Annotation({
    reducer: (x, y) => x.concat(y),
    default: () => [],
  }),
});

// Global variables for agents (will be set in main)
let researcherAgent = null;
let writerAgent = null;
let editorAgent = null;

// 2. Define Node Functions for each Agent
async function researcherNode(state) {
  console.log("\n" + "=".repeat(50));
  console.log("RESEARCHER NODE");
  console.log("=".repeat(50));

  // Get the researcher agent from the closure
  const response = await researcherAgent.invoke({
    messages: state.messages,
  });

  // Debug: Print search results and tool usage
  console.log("\n--- Research Results ---");
  for (const msg of response.messages) {
    // Check for tool calls (AI messages with tool_calls)
    if (msg.tool_calls && msg.tool_calls.length > 0) {
      for (const toolCall of msg.tool_calls) {
        console.log(`\nTool Called: ${toolCall.name || "Unknown"}`);
        console.log(`Arguments: ${JSON.stringify(toolCall.args || {})}`);
      }
    }

    // Check for tool responses (ToolMessage)
    if (msg._getType() === "tool") {
      console.log(`\nTool Response from: ${msg.name || "Unknown Tool"}`);
      const contentPreview =
        msg.content.length > 500
          ? msg.content.substring(0, 500) + "..."
          : msg.content;
      console.log(`Content: ${contentPreview}`);
    }

    // Print AI responses (but not tool calls)
    if (msg._getType() === "ai" && (!msg.tool_calls || msg.tool_calls.length === 0)) {
      console.log(`\nResearcher Response:`);
      console.log(`${msg.content}`);
    }
  }

  console.log("\n" + "=".repeat(50) + "\n");

  // Native handoff: explicitly tell the graph to move to 'writer'
  return new Command({
    update: { messages: response.messages },
    goto: "writer",
  });
}

async function writerNode(state) {
  console.log("\n" + "=".repeat(50));
  console.log("WRITER NODE");
  console.log("=".repeat(50));

  const response = await writerAgent.invoke({
    messages: state.messages,
  });

  // Print the written content
  const finalMessage = response.messages[response.messages.length - 1];
  console.log(`\nWriter Output:`);
  console.log(`${finalMessage.content}`);
  console.log("\n" + "=".repeat(50) + "\n");

  // Native handoff: explicitly tell the graph to move to 'editor'
  return new Command({
    update: { messages: response.messages },
    goto: "editor",
  });
}

async function editorNode(state) {
  console.log("\n" + "=".repeat(50));
  console.log("EDITOR NODE");
  console.log("=".repeat(50));

  const response = await editorAgent.invoke({
    messages: state.messages,
  });

  // Debug: Print editor feedback
  const finalMessage = response.messages[response.messages.length - 1];
  console.log(`\nEditor Feedback:`);
  console.log(`${finalMessage.content}`);

  // Example logic: if editor finds an error, hand back to writer
  if (finalMessage.content.includes("REVISE")) {
    console.log("\n⚠️  Editor requested REVISION - routing back to writer");
    console.log("=".repeat(50) + "\n");
    return new Command({
      update: { messages: response.messages },
      goto: "writer",
    });
  }

  console.log("\n✓ Editor approved - workflow complete");
  console.log("=".repeat(50) + "\n");

  return new Command({
    update: { messages: response.messages },
    goto: "__end__",
  });
}

// Helper function to get user input
function getUserInput(prompt) {
  const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
  });

  return new Promise((resolve) => {
    rl.question(prompt, (answer) => {
      rl.close();
      resolve(answer);
    });
  });
}

async function main() {
  console.log("🤖 JavaScript LangChain Multi-Agent Workflow Starting...\n");

  // Check for required API keys
  if (!process.env.GITHUB_TOKEN) {
    console.error("Error: GITHUB_TOKEN not found.");
    console.log("Add GITHUB_TOKEN=your-token to a .env file");
    process.exit(1);
  }

  if (!process.env.TAVILY_API_KEY) {
    console.error("Error: TAVILY_API_KEY not found.");
    console.log("Add TAVILY_API_KEY=your-key to a .env file");
    console.log("Get your API key from: https://app.tavily.com/");
    process.exit(1);
  }

  // Initialize LLM
  const llm = new ChatOpenAI({
    modelName: "openai/gpt-4o-mini",
    temperature: 0.7,
    configuration: {
      baseURL: "https://models.github.ai/inference",
      apiKey: process.env.GITHUB_TOKEN,
    },
  });

  // Load prompts from your local filesystem
  const researcherData = JSON.parse(
    readFileSync("templates/researcher.json", "utf-8")
  );
  const researcherPrompt =
    researcherData.template || "You are a helpful research assistant.";

  const writerData = JSON.parse(
    readFileSync("templates/writer.json", "utf-8")
  );
  const writerPrompt =
    writerData.template || "You are a helpful writing assistant.";

  const editorData = JSON.parse(
    readFileSync("templates/editor.json", "utf-8")
  );
  const editorPrompt =
    editorData.template || "You are a helpful editing assistant.";

  // Get Tavily API key from environment
  const tavilyApiKey = process.env.TAVILY_API_KEY;

  // Create MCP client for Tavily
  const researchClient = new MultiServerMCPClient({
    tavily: {
      transport: "http",
      url: `https://mcp.tavily.com/mcp/?tavilyApiKey=${tavilyApiKey}`,
    },
  });

  // Get tools from the client (await because it's async)
  const researcherTools = await researchClient.getTools();

  console.log(`Research tools: ${researcherTools.map((t) => t.name).join(", ")}`);

  // Create agents using createAgent
  researcherAgent = createAgent({
    model: llm,
    tools: researcherTools,
    systemPrompt: researcherPrompt,
  });

  // Writer and editor don't need tools
  writerAgent = createAgent({
    model: llm,
    tools: [],
    systemPrompt: writerPrompt,
  });

  editorAgent = createAgent({
    model: llm,
    tools: [],
    systemPrompt: editorPrompt,
  });

  // 3. Build the Graph with edges for Command-based routing
  const builder = new StateGraph(StateAnnotation)
    .addNode("researcher", researcherNode, { ends: ["writer"] })
    .addNode("writer", writerNode, { ends: ["editor"] })
    .addNode("editor", editorNode, { ends: ["__end__"] })
    .addEdge(START, "researcher")

  const graph = builder.compile();

  // Run the workflow
  console.log("\n" + "=".repeat(50));
  console.log("Starting Multi-Agent Content Creation Workflow");
  console.log("=".repeat(50) + "\n");

  const userInput = await getUserInput(
    "Enter the topic that you would like to research: "
  );
  const initialMessage = new HumanMessage({ content: userInput });
  const result = await graph.invoke({ messages: [initialMessage] });

  console.log("\n" + "=".repeat(50));
  console.log("Workflow Complete");
  console.log("=".repeat(50) + "\n");
  console.log("Final Output:");
  console.log(
    result.messages.length > 0
      ? result.messages[result.messages.length - 1].content
      : "No output"
  );
}

main().catch(console.error);
