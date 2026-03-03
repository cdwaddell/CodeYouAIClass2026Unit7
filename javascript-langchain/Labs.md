# Lab: Building a Multi-Agent Content Creation Workflow

## Overview

In this lab, you'll build a sophisticated multi-agent system that collaborates to research topics and create polished content. You'll learn how to:

- Orchestrate multiple AI agents working together
- Use LangGraph to manage workflow state and transitions
- Integrate external tools (web search) with AI agents
- Create agent-to-agent handoffs with feedback loops
- Load prompts from template files for better organization

## The Workflow

Your multi-agent system will have three specialized agents:

1. **Researcher Agent** - Searches the web for information on a topic
2. **Writer Agent** - Creates engaging content from the research
3. **Editor Agent** - Reviews the content and requests revisions if needed

The workflow follows this pattern:
```
User Input → Researcher → Writer → Editor → ✓ Done
                            ↑_________|
                          (if revision needed)
```

## Prerequisites

Before starting, ensure you have:
- Node.js 16 or higher installed
- A `.env` file with `GITHUB_TOKEN` and `TAVILY_API_KEY`
- The required packages installed (see `package.json`)

---

## Part 1: Setting Up the Orchestration

In this section, you'll set up the core orchestration framework using LangGraph. This creates the "skeleton" that your agents will run within.

### Understanding the State Graph

LangGraph uses a **StateGraph** to manage how data flows between agents. Think of it like a flowchart where:
- **Nodes** are the agents (researcher, writer, editor)
- **Edges** are the transitions between agents
- **State** is the shared data (conversation messages) that flows through the graph

### Step 1.1: Define the Shared State

At the top of your `app.js` file, after the imports, add the state annotation:

```javascript
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
```

**What this does:**
- **StateAnnotation:** Defines the structure of data flowing through the graph
- **messages:** An array that accumulates all conversation messages
- **reducer:** Specifies how to merge new messages (concatenation)
- **default:** Starts with an empty array

### Step 1.2: Set Up Global Agent Variables

Since node functions need to access the agents, declare them globally:

```javascript
// Global variables for agents (will be set in main)
let researcherAgent = null;
let writerAgent = null;
let editorAgent = null;
```

**What this does:** Creates placeholders for agents that will be initialized in the `main()` function and accessed by node functions.

### Step 1.3: Create Helper Function for User Input

Add this helper function before `main()`:

```javascript
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
```

**What this does:** Provides a way to get user input from the console using promises, making it compatible with async/await.

### Step 1.4: Initialize the Main Function Structure

Now set up the `main()` function with the basic structure:

```javascript
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

  // TODO: Get tools and create agents (Part 2)
  // TODO: Build the graph (Part 2)
  // TODO: Get user input and run workflow (Part 2)
}

main().catch(console.error);
```

**What this does:**
- **API key validation:** Ensures required environment variables are set
- **LLM initialization:** Configures the ChatOpenAI model with GitHub Models
- **Template loading:** Reads agent system prompts from JSON files
- **MCP client setup:** Connects to Tavily for web search capabilities

### Step 1.5: Test Your Setup

At this point, you should install dependencies and test:

```bash
npm install
node app.js
```

**Expected behavior:** The script should validate your API keys and load templates. If keys are missing, you'll see clear error messages.

**Checkpoint:** You now have the orchestration framework in place! The state is defined, templates are loaded, and the structure is ready for your agents.

---

## Part 2: Adding the Researcher Agent

Now you'll implement your first agent - the researcher - which can search the web for information.

### Understanding Agent Node Functions

Each agent in LangGraph is represented by a **node function**. These functions:
1. Receive the current state (conversation history)
2. Call the agent to process the messages
3. Return a `Command` that updates state and routes to the next agent

### Step 2.1: Implement the Researcher Node Function

Add this function after the global agent variables and before the helper function:

```javascript
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
```

**What this does:**
- **Receives state:** Gets the conversation history from the graph
- **Invokes agent:** Calls the researcher agent to process messages and use tools
- **Logs activity:** Prints what tools were called and what was found
- **Returns Command:** Updates the state with new messages and routes to the writer node

**Key concept - Command-based routing:** The `Command` object tells LangGraph:
1. **update:** What data to add to the state (new messages from the agent)
2. **goto:** Which node to execute next (the writer)

### Step 2.2: Create the Researcher Agent and Get Tools

Find the TODO for getting tools and creating agents in `main()` and replace it with:

```javascript
  // Get tools from the client (await because it's async)
  const researcherTools = await researchClient.getTools();

  console.log(`Research tools: ${researcherTools.map((t) => t.name).join(", ")}`);

  // Create agents using createAgent
  researcherAgent = createAgent({
    model: llm,
    tools: researcherTools,
    systemPrompt: researcherPrompt,
  });

  // TODO: Create writer and editor agents (Part 3)
```

**What this does:**
- **Gets tools:** Retrieves available search tools from the Tavily MCP client
- **Creates agent:** Initializes the researcher with the LLM, tools, and system prompt
- **Logs tools:** Shows which tools are available to the researcher

### Step 2.3: Build the Graph

Add the graph building code after agent creation. Find the TODO for building the graph and add:

```javascript
  // 3. Build the Graph with edges for Command-based routing
  const builder = new StateGraph(StateAnnotation)
    .addNode("researcher", researcherNode, { ends: ["writer"] })
    // TODO: Add writer and editor nodes (Part 3)
    .addEdge(START, "researcher");

  const graph = builder.compile();
```

**What this does:**
- **Creates StateGraph:** Initializes the graph with our state structure
- **Adds researcher node:** Registers the researcher function with possible destinations
- **Sets start edge:** Workflow begins at the researcher node
- **Compiles:** Converts the builder into an executable graph

### Step 2.4: Get User Input and Run the Workflow

Find the TODO for running the workflow and add:

```javascript
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
```

**What this does:**
- **Gets user input:** Prompts for a research topic
- **Creates initial message:** Wraps the topic in a HumanMessage
- **Invokes graph:** Starts the workflow with the initial message
- **Waits for completion:** The `await` ensures all agents finish
- **Prints result:** Shows the final message from the workflow

### Step 2.5: Test the Researcher Agent

Try running your code now:

```bash
node app.js
```

**What to expect:**
- The researcher will receive your topic
- It will use Tavily search tools to find information
- You'll see tool calls and search results printed
- The workflow will try to hand off to the writer (which doesn't exist yet, so it will error)

**Checkpoint:** You now have a working researcher agent that can search the web! In the next section, you'll add the writer and editor agents.

---

## Part 3: Adding Writer and Editor Agents

Now you'll complete the workflow by adding the writer (who creates content) and editor (who reviews and potentially requests revisions).

### Step 3.1: Implement the Writer Node Function

Add this function after `researcherNode()`:

```javascript
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
```

**What this does:**
- Takes the research findings from the state
- Has the writer agent create engaging content
- Prints the drafted content
- Routes to the editor for review

### Step 3.2: Implement the Editor Node Function

Add this function after `writerNode()`:

```javascript
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
```

**What this does:**
- Reviews the writer's content
- Checks if the word "REVISE" appears in the editor's response
- **If revision needed:** Routes back to the writer with feedback
- **If approved:** Routes to `__end__` to complete the workflow

**Key concept - Conditional routing:** The editor can create a **feedback loop** by sending work back to the writer. This loop continues until the editor is satisfied.

### Step 3.3: Create Writer and Editor Agents

Find the TODO for creating writer and editor agents in `main()` and replace it with:

```javascript
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
```

**What this does:**
- Creates both agents with no tools (they work with text only)
- Uses their respective system prompts from the template files

### Step 3.4: Complete the Graph

Update the graph building section to include all three nodes. Find the graph builder code and update it:

```javascript
  // 3. Build the Graph with edges for Command-based routing
  const builder = new StateGraph(StateAnnotation)
    .addNode("researcher", researcherNode, { ends: ["writer"] })
    .addNode("writer", writerNode, { ends: ["editor"] })
    .addNode("editor", editorNode, { ends: ["__end__"] })
    .addEdge(START, "researcher");

  const graph = builder.compile();
```

**What this does:**
- Adds all three node functions to the graph
- Specifies possible destinations for each node (via `ends` parameter)
- The START edge routes to the researcher
- Command-based routing in node functions handles the rest

### Step 3.5: Test the Complete Workflow

Run your complete system:

```bash
node app.js
```

**Checkpoint:** You now have a fully functional multi-agent content creation system!

---

## Part 4: Testing Your Multi-Agent System

### Test Run 1: Basic Workflow

Try a topic like: **"The benefits of urban gardens"**

**What you should see:**

1. **Researcher Node:**
   - Tool calls to Tavily search
   - Search results about urban gardens
   - Summary of findings

2. **Writer Node:**
   - Well-structured article with introduction
   - Body sections with research-backed points
   - Engaging conclusion

3. **Editor Node:**
   - Review of the content
   - Either approval or revision request with specific feedback

4. **Final Output:**
   - The approved article (or revised version if editor requested changes)

### Test Run 2: Trigger a Revision

Try a complex or niche topic that might produce content the editor wants revised:

**Topic:** "Quantum entanglement applications in cryptography"

Watch to see if the editor requests a REVISE, sending the workflow back to the writer.

### Understanding the Output

Your terminal output should show:

```
🤖 JavaScript LangChain Multi-Agent Workflow Starting...

Research tools: tavily_search, ...

==================================================
Starting Multi-Agent Content Creation Workflow
==================================================

Enter the topic that you would like to research: [your topic]

==================================================
RESEARCHER NODE
==================================================

--- Research Results ---

Tool Called: tavily_search
Arguments: {"query":"benefits of urban gardens"}

Tool Response from: tavily_search
Content: [search results...]

Researcher Response:
Based on my research, I found...

==================================================

==================================================
WRITER NODE
==================================================

Writer Output:
[Drafted article content...]

==================================================

==================================================
EDITOR NODE
==================================================

Editor Feedback:
This content is well-structured and accurate...

✓ Editor approved - workflow complete
==================================================

==================================================
Workflow Complete
==================================================

Final Output:
[The final approved article]
```

---

## Part 5: Understanding Key Concepts

### 1. StateGraph and Message Flow

The `StateAnnotation` manages shared state between agents:

```javascript
const StateAnnotation = Annotation.Root({
  messages: Annotation({
    reducer: (x, y) => x.concat(y),
    default: () => [],
  }),
});
```

- **messages:** A list that grows as agents add their responses
- **reducer:** A function that concatenates new messages to existing ones
- Each agent can see all previous messages (full conversation history)

### 2. Command-Based Routing

The `Command` object gives nodes control over workflow navigation:

```javascript
return new Command({
  update: { messages: response.messages },
  goto: "editor",
});
```

- **update:** Data to add to the state
- **goto:** Next node to execute (or `"__end__"` to finish)

This enables **dynamic routing** - decisions made at runtime based on agent output.

### 3. Agent Creation with Tools

```javascript
researcherAgent = createAgent({
  model: llm,
  tools: researcherTools,  // External capabilities
  systemPrompt: researcherPrompt  // Behavior instructions
});
```

- **model:** The LLM that powers the agent's intelligence
- **tools:** External functions the agent can call (web search, calculators, etc.)
- **systemPrompt:** Instructions that shape the agent's behavior and role

### 4. Templates for Maintainability

Storing prompts in JSON files (`templates/*.json`) provides:
- **Separation of concerns:** Logic vs. instructions
- **Easy updates:** Modify agent behavior without changing code
- **Reusability:** Share prompts across different implementations

### 5. Async/Await Pattern

```javascript
const response = await researcherAgent.invoke({
  messages: state.messages,
});
```

JavaScript's async/await allows:
- **Non-blocking I/O:** The program continues while waiting for API responses
- **Sequential code flow:** Async code reads like synchronous code
- **Error handling:** Use try/catch with async operations

---

## Part 6: Extension Challenges

Ready to go further? Try these enhancements:

### Challenge 1: Add a Fact-Checker Agent

Create a fourth agent that:
- Runs between the researcher and writer
- Verifies claims using additional searches
- Flags any questionable information

**Hints:**
- Create a `factCheckerNode()` function
- Give it access to `researcherTools`
- Update the routing: researcher → fact_checker → writer

### Challenge 2: Add Multiple Revision Rounds Limit

Prevent infinite loops by:
- Adding a `revisionCount` field to the StateAnnotation
- Incrementing it in the editor node
- Forcing approval after 2-3 revisions

**Hint:** Modify the `StateAnnotation` and update `editorNode()` logic.

### Challenge 3: Save Output to File

After workflow completion:
- Extract the final article
- Save it to a markdown file with the topic as the filename
- Include metadata (timestamp, number of revisions, etc.)

### Challenge 4: Add Source Citations

Enhance the researcher to:
- Track which sources provided which information
- Pass source URLs to the writer
- Have the writer include citations in the article

### Challenge 5: Multi-Language Support

Extend the workflow to:
- Accept a target language parameter
- Have the writer produce content in that language
- Ensure the editor understands the target language

---

## Part 7: Review and Learning Outcomes

Congratulations! You've built a sophisticated multi-agent system. Let's review what you learned:

### Core Concepts Mastered

✓ **Multi-agent orchestration** - Coordinating multiple AI agents with different roles  
✓ **StateGraph** - Managing shared state across a workflow  
✓ **Command-based routing** - Dynamic workflow navigation based on agent decisions  
✓ **Tool integration** - Connecting external services (Tavily) to agents  
✓ **Feedback loops** - Creating revision cycles between agents  
✓ **Template-based prompts** - Organizing agent instructions in separate files  
✓ **Async programming** - Using JavaScript's async/await for I/O operations  

### Architecture Patterns

You implemented several important patterns:

1. **Separation of Concerns:** Each agent has a single, clear responsibility
2. **Pipeline Pattern:** Data flows through a sequence of processing stages
3. **Feedback Loop:** The editor can send work back for improvement
4. **External Tool Integration:** Agents augmented with real-world capabilities

### Real-World Applications

This pattern can be adapted for:

- **Content creation:** Blog posts, reports, marketing copy
- **Research synthesis:** Academic papers, market analysis
- **Code generation:** Spec → Implementation → Review → Testing
- **Data analysis:** Collection → Processing → Visualization → Insights
- **Customer service:** Question → Research → Response → Quality Check

### Next Steps

To continue your learning:

1. **Experiment with prompts** - Modify the templates to change agent behavior
2. **Try different topics** - See how the system handles various domains
3. **Add complexity** - Implement the extension challenges
4. **Explore LangGraph docs** - Learn about more advanced routing and state management
5. **Build your own workflow** - Apply these patterns to your own use cases

---

## Troubleshooting

### Common Issues

**Issue:** `Cannot find module '@langchain/mcp-adapters'`  
**Solution:** Run `npm install` to install dependencies

**Issue:** `Error: TAVILY_API_KEY not found`  
**Solution:** Create a `.env` file with your Tavily API key from https://app.tavily.com/

**Issue:** `researcherAgent is not defined`  
**Solution:** Ensure you declared the global variables at the top of the file

**Issue:** Editor keeps requesting revisions  
**Solution:** This can happen with very niche topics. Try a more mainstream topic, or adjust the editor's system prompt to be less critical.

**Issue:** Slow execution  
**Solution:** The workflow makes multiple API calls. This is expected. Each agent call and tool use takes a few seconds.

### Getting Help

- Review the completed reference implementation if you get stuck
- Check the LangGraph documentation: https://langchain-ai.github.io/langgraph/
- Ask your instructor or use GitHub Copilot for specific questions

---

## Summary

You've successfully built a multi-agent content creation system! This lab demonstrated:

- How to design and implement multi-agent workflows
- How to use LangGraph for orchestration and state management
- How to integrate external tools with AI agents
- How to create feedback loops and conditional routing
- How to organize agent instructions using templates

This foundation will serve you well as you build more complex AI systems. Happy coding! 🚀
