# JavaScript LangChain Agent

A command-line AI agent built with JavaScript and LangChain that demonstrates the power of AI tool use through modular tools. This educational project shows how AI models can intelligently invoke custom functions to perform tasks they couldn't accomplish on their own, such as precise calculations, real-time data access, and string manipulation.

## Getting Started

For complete setup instructions, prerequisites, and a step-by-step lab guide, see [INSTRUCTIONS.md](INSTRUCTIONS.md).

## About This Project

This application showcases how to build AI agents that extend language model capabilities through **tool use** (also known as function calling). Instead of the AI hallucinating answers or approximating results, it can invoke specialized tools to get accurate, real-time information.

### What is LangChain?

**LangChain** is an open-source framework that enables developers to build applications powered by large language models. It provides:
- **Agents**: Orchestration logic that determines which tools to use and when
- **Tools**: Functions the AI can discover and invoke to accomplish tasks
- **Chains**: Sequences of operations that can be composed together
- **Memory**: Conversation history and context management
- **Integrations**: Connectors for various AI services and data sources

Think of it as a framework that makes AI models "actionable" - you define what the AI can do, and it figures out when and how to do it.

### What are Tools?

Tools are functions that extend the AI's capabilities. Each tool:
- Has a name that identifies it
- Has a description explaining what it does and when to use it
- Accepts input parameters that the AI provides
- Returns results that the AI incorporates into its response

**Example:** When you ask "What time is it?", the AI:
1. Recognizes it needs current time information
2. Finds the `get_current_time` tool that matches the need
3. Invokes the tool to get real system time
4. Incorporates the result into a natural language response

This is fundamentally different from the AI guessing or using outdated training data.

### How Tool Use Works

1. **User Query**: "What is 25 * 4 + 10?"
2. **AI Analysis**: The model recognizes this requires calculation
3. **Tool Discovery**: Finds `Calculator` tool matches the need
4. **Tool Invocation**: Agent executes the calculator with the expression
5. **Result Integration**: AI receives "110" and responds naturally
6. **User Response**: "The answer is 110"

The AI doesn't perform the calculation - it orchestrates the right tool for the job.

## Features

This project demonstrates:
- **LangChain Integration**: Popular AI orchestration framework for JavaScript
- **GitHub Models**: Access to OpenAI's GPT-4o model via GitHub's free tier
- **Tool Architecture**: Three example tools showing different use cases:
  - **Calculator**: Precise mathematical calculations using expr-eval
  - **get_current_time**: Real-time system data access
  - **reverse_string**: Text manipulation operations
- **Automatic Tool Selection**: The agent intelligently selects and invokes tools
- **OpenAI Functions Agent**: Uses function calling API for reliable tool use
- **Error Handling**: Graceful degradation with informative error messages
- **Environment Configuration**: Secure token management via `.env` files

## How It Works

The application creates an agent executor configured with:
1. **AI Model**: OpenAI chat model via GitHub Models endpoint
2. **Tools**: Registered collection of callable functions
3. **Agent Type**: OpenAI Functions agent for automatic tool invocation
4. **Verbose Mode**: Displays reasoning and tool selection process

When you send a query, the agent:
- Analyzes what the user wants
- Determines if any registered tools can help
- Invokes those tools with appropriate parameters
- Synthesizes results into a natural language response

## Project Structure

```
javascript-langchain/
├── app.js                # Main application, agent setup, and tool definitions
├── package.json          # Dependencies and project configuration
├── .env                  # Environment variables (git-ignored)
├── INSTRUCTIONS.md       # Complete lab guide
└── README.md            # This file
```

## Tool Examples

### Calculator Tool
Evaluates mathematical expressions with precision:
```javascript
import { Calculator } from "@langchain/community/tools/calculator";

const tools = [
  new Calculator(), // Handles math expressions like "25 * 4 + 10"
];
```

### Time Tool
Provides access to real-time system information:
```javascript
new DynamicTool({
  name: "get_current_time",
  description: "Returns the current date and time. Use this when the user asks about the current time or date.",
  func: async () => {
    return new Date().toString();
  },
})
```

### String Reversal Tool
Performs text transformations:
```javascript
new DynamicTool({
  name: "reverse_string",
  description: "Reverses a string. Input should be a single string.",
  func: async (input) => {
    return input.split("").reverse().join("");
  },
})
```

## Example Queries and Behavior

| Query | Tool Used | Behavior |
|-------|-----------|----------|
| "What time is it?" | get_current_time | Gets actual system time, not training data |
| "Calculate 25 * 4 + 10" | Calculator | Returns precise result: 110 |
| "Reverse 'Hello World'" | reverse_string | Returns: "dlroW olleH" |
| "What's the weather?" | None | AI responds with limitation (no weather tool) |

## Customization

### Adding New Tools

Tools extend what the AI can do. To create a new tool:

1. **Create a DynamicTool** with a descriptive name and description:

```javascript
import { DynamicTool } from "@langchain/core/tools";

const weatherTool = new DynamicTool({
  name: "get_weather",
  description: "Gets weather information for a specific date in yyyy-MM-dd format. Returns temperature and conditions.",
  func: async (date) => {
    // Your logic here - could call a weather API
    const today = new Date().toISOString().split('T')[0];
    if (date === today) {
      return "Sunny, 72°F";
    }
    return "Rainy, 55°F";
  },
});
```

2. **Add it to the tools array**:
```javascript
const tools = [
  new Calculator(),
  getCurrentTimeTool,
  reverseStringTool,
  weatherTool,  // Your new tool
];
```

3. **The AI automatically discovers it** - no prompt engineering needed!

### Changing AI Models

The application uses GitHub Models for free access to OpenAI models. You can switch models by changing the `modelName`:

```javascript
const model = new ChatOpenAI({
  modelName: "openai/gpt-4o",  // Try: gpt-4o-mini, gpt-4-turbo
  temperature: 0,
  configuration: {
    baseURL: "https://models.github.ai/inference",
    apiKey: process.env.GITHUB_TOKEN,
  },
});
```

Available models: https://github.com/marketplace/models

## Technology Stack

### Core Dependencies

- **@langchain/openai**: OpenAI integration for chat models
- **@langchain/community**: Community-contributed tools like Calculator
- **@langchain/core**: Core LangChain utilities for agents and tools
- **langchain**: Main LangChain framework for orchestration
- **dotenv**: Environment variable management for secure token storage

### Runtime

- **Node.js 18+**: Required for ES6 module support and modern JavaScript features
- **npm**: Package management and dependency installation

## Key Concepts

### Tool Use vs. Prompt Engineering

**Without Tool Use (Unreliable):**
```
User: "What is 25 * 4 + 10?"
AI: "I believe the answer is approximately 110" ← May be wrong
```

**With Tool Use (Accurate):**
```
User: "What is 25 * 4 + 10?"
AI: [Invokes Calculator tool with "25 * 4 + 10"]
Tool: Returns "110"
AI: "The answer is 110" ← Always correct
```

### Automatic Tool Selection

The AI model itself decides when to use tools - you don't need to:
- Parse user intent manually
- Write if/else logic for different query types
- Specify which tool to call

The agent handles this through:
1. **Tool descriptions**: AI reads tool metadata to understand capabilities
2. **Parameter schemas**: AI understands what inputs are needed
3. **Automatic invocation**: Agent executes tools and returns results
4. **Response synthesis**: AI incorporates results naturally

### Multi-Tool Chaining

The AI can chain multiple tools in one query:

```
Query: "What's the weather like today?"
Step 1: Call get_current_time() → "2026-01-13"
Step 2: Call get_weather("2026-01-13") → "Sunny, 72°F"
Response: "Today's weather is sunny with a temperature of 72°F"
```

## Learning Resources

- **LangChain Documentation**: https://js.langchain.com/docs
- **GitHub Models**: https://github.com/marketplace/models
- **OpenAI Function Calling Guide**: https://platform.openai.com/docs/guides/function-calling
- **Lab Instructions**: [INSTRUCTIONS.md](INSTRUCTIONS.md)

## Educational Goals

This project teaches:
1. **AI Orchestration**: How to coordinate LLMs with custom code
2. **Tool Architecture**: Building modular, reusable AI capabilities
3. **Function Calling**: The difference between AI assistance and AI agents
4. **Practical Integration**: Real-world patterns for AI-powered applications

## License

This is an educational project for learning AI agent development with LangChain.

---

**Ready to get started?** See [INSTRUCTIONS.md](INSTRUCTIONS.md) for the complete step-by-step lab guide
