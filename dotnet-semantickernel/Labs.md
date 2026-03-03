# Lab: Building a Multi-Agent Content Creation Workflow with Semantic Kernel

## Overview

In this lab, you'll build a sophisticated multi-agent system using Microsoft Semantic Kernel that creates and refines content through collaboration between three AI agents:

- **Researcher** 🔍: Gathers information using web search tools
- **Writer** ✍️: Creates content based on research
- **Editor** 📝: Reviews and approves or requests revisions

By the end of this lab, you'll understand:
- How to create and orchestrate multiple AI agents
- How to integrate external tools (MCP) with agents
- How to implement custom agent selection and termination strategies
- How to build sequential workflows with agent handoff

## Prerequisites

Before starting, ensure you have:
- .NET 9.0+ SDK installed
- GitHub Token (for GitHub Models API access)
- Tavily API Key (for web search - get from https://app.tavily.com/)

Set your environment variables:
```powershell
$env:GITHUB_TOKEN="your-github-token"
$env:TAVILY_API_KEY="your-tavily-api-key"
```

---

## Step 1: Setup the Basic Orchestration Structure

In this step, you'll create the foundation of your multi-agent system, including configuration, kernel creation, and the basic workflow loop.

### 1.1: Add Required Using Statements

Start by adding all necessary namespaces at the top of your `Program.cs`:

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using ModelContextProtocol.Client;
using Microsoft.Extensions.AI;

#pragma warning disable SKEXP0110 // Agents are experimental
```

**Why?** These namespaces provide access to Semantic Kernel's agent framework, chat completion services, configuration management, and MCP client functionality.

### 1.2: Create the Program Structure

Create your main program class with agent name constants:

```csharp
namespace SemanticKernelAgent
{
    class Program
    {
        private const string ResearcherName = "Researcher";
        private const string WriterName = "Writer";
        private const string EditorName = "Editor";

        static async Task Main(string[] args)
        {
            Console.WriteLine("🤖 C# Semantic Kernel Multi-Agent Workflow Starting...\n");
            
            // We'll add the rest here
        }
    }
}
```

**Why constants?** Using constants for agent names ensures consistency throughout the code and makes it easy to reference agents in your orchestration logic.

### 1.3: Load Configuration

Add configuration loading inside `Main`:

```csharp
// Load configuration
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .Build();

var githubToken = configuration["GITHUB_TOKEN"];
var tavilyApiKey = configuration["TAVILY_API_KEY"];

if (string.IsNullOrEmpty(githubToken))
{
    Console.WriteLine("❌ Error: GITHUB_TOKEN not found.");
    Console.WriteLine("Set it using: $env:GITHUB_TOKEN=\"your-token\"");
    return;
}

if (string.IsNullOrEmpty(tavilyApiKey))
{
    Console.WriteLine("❌ Error: TAVILY_API_KEY not found.");
    Console.WriteLine("Set it using: $env:TAVILY_API_KEY=\"your-key\"");
    Console.WriteLine("Get your API key from: https://app.tavily.com/");
    return;
}
```

**What's happening?** The configuration builder checks environment variables and user secrets for your API keys. This is more secure than hardcoding credentials.

### 1.4: Create Helper Methods for Kernel and Template Loading

Add these helper methods to the `Program` class:

```csharp
private static Kernel CreateKernel(string githubToken)
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion(
        modelId: "openai/gpt-4o-mini",
        apiKey: githubToken,
        endpoint: new Uri("https://models.github.ai/inference"));
    return builder.Build();
}

private static async Task<string> LoadPromptFromTemplate(string templatePath)
{
    var json = await File.ReadAllTextAsync(templatePath);
    var template = JsonSerializer.Deserialize<AgentTemplate>(json);
    return template?.Template ?? "You are a helpful assistant.";
}
```

And add the template class:

```csharp
// Template model for JSON deserialization
class AgentTemplate
{
    public string? Template { get; set; }
}
```

**Understanding Kernels:** Each agent gets its own `Kernel`, which is like a container that holds the AI model connection and any plugins/tools the agent can use.

### 1.5: Setup User Input

Add code to get the user's research topic:

```csharp
// Get user input
Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine("Multi-Agent Content Creation Workflow");
Console.WriteLine(new string('=', 50) + "\n");

Console.Write("Enter the topic you would like to research: ");
var userInput = Console.ReadLine();

if (string.IsNullOrEmpty(userInput))
{
    Console.WriteLine("No topic provided. Exiting.");
    return;
}
```

### 1.6: Add a Message Display Helper

Add this helper method to display agent messages with emojis:

```csharp
private static void WriteMessage(ChatMessageContent message)
{
    var authorName = message.AuthorName ?? message.Role.ToString();
    var emoji = authorName switch
    {
        "User" => "🔵",
        ResearcherName => "🔍",
        WriterName => "✍️",
        EditorName => "📝",
        _ => "🤖"
    };

    Console.WriteLine($"\n{emoji} {authorName}:");
    Console.WriteLine(new string('-', 50));
    Console.WriteLine(message.Content);
    Console.WriteLine();
}
```

**Why this matters:** Clear visual feedback helps you understand the agent workflow as it executes.

### 1.7: Test Your Progress

At this point, you can test that configuration loading works:

```csharp
// Temporary test - we'll replace this later
Console.WriteLine($"✓ GitHub Token loaded: {githubToken.Substring(0, 10)}...");
Console.WriteLine($"✓ Tavily API Key loaded: {tavilyApiKey.Substring(0, 10)}...");
```

Run your program to verify everything compiles and configuration loads correctly.

---

## Step 2: Setup the Researcher Agent with MCP Tools

Now we'll create the Researcher agent and connect it to the Tavily search service using the Model Context Protocol (MCP).

### 2.1: Load Agent Templates

Add this code after configuration loading:

```csharp
// Load agent prompts from templates
var researcherPrompt = await LoadPromptFromTemplate("templates/researcher.json");
var writerPrompt = await LoadPromptFromTemplate("templates/writer.json");
var editorPrompt = await LoadPromptFromTemplate("templates/editor.json");
```

**Note:** Make sure your `templates/` folder contains the three JSON files with agent instructions.

### 2.2: Create Kernels for Each Agent

```csharp
// Create kernels for each agent
var researcherKernel = CreateKernel(githubToken);
var writerKernel = CreateKernel(githubToken);
var editorKernel = CreateKernel(githubToken);
```

**Why separate kernels?** Each agent can have its own set of tools and configurations. The Researcher will get search tools, while Writer and Editor won't need them.

### 2.3: Connect to Tavily MCP Server

Add the MCP client connection helper method:

```csharp
private static async Task<McpClient> ConnectToTavilyMcpAsync(string apiKey)
{
    // Create HttpClientTransport to connect to Tavily's MCP endpoint
    var transport = new HttpClientTransport(new HttpClientTransportOptions
    {
        Endpoint = new Uri($"https://mcp.tavily.com/mcp/?tavilyApiKey={apiKey}")
    });

    // Create and connect MCP client
    var client = await McpClient.CreateAsync(transport);
    return client;
}
```

Now use it in `Main`:

```csharp
// Connect to Tavily MCP server and add tools
Console.WriteLine("Connecting to Tavily MCP server...");
var mcpClient = await ConnectToTavilyMcpAsync(tavilyApiKey);
var mcpTools = await mcpClient.ListToolsAsync();

Console.WriteLine($"✓ Connected to Tavily MCP server");
Console.WriteLine($"  Available MCP tools: {string.Join(", ", mcpTools.Select(t => t.Name))}");
Console.WriteLine();
```

**What is MCP?** The Model Context Protocol is a standardized way for AI agents to access external tools and data sources. Tavily provides web search capabilities through MCP.

### 2.4: Add MCP Tools to the Researcher Kernel

```csharp
// Convert MCP tools (AIFunction) to Semantic Kernel functions
var kernelFunctions = new List<KernelFunction>();

researcherKernel.Plugins.AddFromFunctions("tavily", kernelFunctions);
```

**Understanding Tool Integration:** The MCP tools are added as a plugin to the Researcher's kernel. This makes them available for the agent to call during execution.

### 2.5: Create the Researcher Agent

```csharp
// Create agents
var researcherAgent = new ChatCompletionAgent
{
    Name = ResearcherName,
    Instructions = researcherPrompt,
    Kernel = researcherKernel,
    Arguments = new KernelArguments(new OpenAIPromptExecutionSettings 
    { 
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() 
    })
};
```

**Key Configuration:**
- `Name`: Identifies the agent in chat history
- `Instructions`: The system prompt defining the agent's role
- `Kernel`: Contains the AI model and tools
- `FunctionChoiceBehavior.Auto()`: Allows the agent to automatically decide when to use its tools

### 2.6: Test the Researcher Agent

Create a simple test with just the Researcher:

```csharp
// Temporary test - single agent
var testKernel = CreateKernel(githubToken);
var testChat = new AgentGroupChat(researcherAgent);
testChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, userInput));

await foreach (var response in testChat.InvokeAsync())
{
    WriteMessage(response);
    break; // Only get one response for now
}
```

Run this to verify your Researcher agent can access and use the search tools.

---

## Step 3: Add the Writer and Editor Agents

Now we'll add the other two agents to complete the multi-agent system.

### 3.1: Create the Writer Agent

Add this after the Researcher agent creation:

```csharp
var writerAgent = new ChatCompletionAgent
{
    Name = WriterName,
    Instructions = writerPrompt,
    Kernel = writerKernel
};
```

**Notice:** The Writer doesn't need `FunctionChoiceBehavior` because it doesn't use external tools. It focuses on creating content from the Researcher's findings.

### 3.2: Create the Editor Agent

```csharp
var editorAgent = new ChatCompletionAgent
{
    Name = EditorName,
    Instructions = editorPrompt,
    Kernel = editorKernel
};
```

**The Editor's Role:** Reviews content and either approves it or sends it back to the Writer for revisions.

### 3.3: Create the Multi-Agent Group Chat

Now create the orchestrated chat with all three agents:

```csharp
// Create the agent group chat with custom orchestration
var chat = new AgentGroupChat(researcherAgent, writerAgent, editorAgent)
{
    ExecutionSettings = new AgentGroupChatSettings
    {
        SelectionStrategy = new SequentialSelectionStrategy(),
        TerminationStrategy = new EditorApprovalTerminationStrategy
        {
            Agents = new[] { editorAgent },
            MaximumIterations = 10
        }
    }
};
```

**Understanding Orchestration:**
- `SelectionStrategy`: Determines which agent runs next
- `TerminationStrategy`: Decides when the workflow is complete
- `MaximumIterations`: Safety limit to prevent infinite loops

### 3.4: Execute the Complete Workflow

Replace your test code with the full workflow execution:

```csharp
// Add user message
chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, userInput));
WriteMessage(new ChatMessageContent(AuthorRole.User, userInput));

// Execute the workflow
await foreach (var response in chat.InvokeAsync())
{
    WriteMessage(response);
}

Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine("Workflow Complete");
Console.WriteLine(new string('=', 50) + "\n");
Console.WriteLine($"Is Complete: {chat.IsComplete}");
```

**What's happening?**
1. The user's topic is added to the chat
2. `InvokeAsync()` runs the agents in sequence
3. Each agent's response is displayed
4. The workflow continues until the Editor approves

---

## Step 4: Implement Custom Orchestration Strategies

The final step is to implement the custom selection and termination strategies that control agent flow.

### 4.1: Create the Sequential Selection Strategy

Add this class inside the `SemanticKernelAgent` namespace:

```csharp
// Custom selection strategy for sequential agent handoff
class SequentialSelectionStrategy : SelectionStrategy
{
    protected override Task<Agent> SelectAgentAsync(
        IReadOnlyList<Agent> agents, 
        IReadOnlyList<ChatMessageContent> history, 
        CancellationToken cancellationToken = default)
    {
        // Determine which agent should go next based on the last agent
        var lastMessage = history.LastOrDefault();
        var lastAgent = lastMessage?.AuthorName;

        Agent? nextAgent = lastAgent switch
        {
            null => agents.First(a => a.Name == "Researcher"), // Start with researcher
            "Researcher" => agents.First(a => a.Name == "Writer"),
            "Writer" => agents.First(a => a.Name == "Editor"),
            "Editor" => agents.First(a => a.Name == "Writer"), // Editor can send back to writer
            _ => agents.First(a => a.Name == "Researcher")
        };

        Console.WriteLine($"\n>>> Routing to: {nextAgent.Name}");
        return Task.FromResult(nextAgent);
    }
}
```

**How it works:**
- Examines the chat history to see who went last
- Routes to the next agent in sequence: Researcher → Writer → Editor
- If Editor requests revisions, routes back to Writer
- Provides visual feedback about routing decisions

### 4.2: Create the Editor Approval Termination Strategy

Add this class:

```csharp
// Custom termination strategy that checks for editor approval
class EditorApprovalTerminationStrategy : TerminationStrategy
{
    protected override Task<bool> ShouldAgentTerminateAsync(
        Agent agent, 
        IReadOnlyList<ChatMessageContent> history, 
        CancellationToken cancellationToken = default)
    {
        // Only the editor can terminate
        if (agent.Name != "Editor")
            return Task.FromResult(false);

        // Check if the last message from editor contains "REVISE"
        var lastMessage = history.LastOrDefault(m => m.AuthorName == "Editor");
        if (lastMessage?.Content?.Contains("REVISE", StringComparison.OrdinalIgnoreCase) == true)
        {
            Console.WriteLine("⚠️  Editor requested REVISION - routing back to writer");
            return Task.FromResult(false); // Continue workflow
        }

        Console.WriteLine("✓ Editor approved - workflow complete");
        return Task.FromResult(true); // Terminate workflow
    }
}
```

**How it works:**
- Only allows the Editor agent to terminate the workflow
- Checks if the Editor's message contains "REVISE"
- If revision is needed, continues the workflow
- If approved, terminates successfully

### 4.3: Understanding the Complete Flow

Here's how the agents work together:

```
User Input
    ↓
Researcher (searches web, gathers info)
    ↓
Writer (creates content from research)
    ↓
Editor (reviews content)
    ↓
    ├─ Approved → Workflow Complete ✓
    └─ Needs Revision → Back to Writer → Editor → ...
```

---

## Step 5: Test Your Complete Multi-Agent System

### 5.1: Build and Run

Build your project:
```powershell
dotnet build
```

Run your application:
```powershell
dotnet run
```

### 5.2: Try Different Topics

Test with various topics to see how agents collaborate:
- "The history of artificial intelligence"
- "Best practices for sustainable living"
- "Latest developments in quantum computing"

### 5.3: Observe the Agent Interactions

Watch for:
- 🔍 Researcher using search tools to gather information
- ✍️ Writer creating structured content
- 📝 Editor reviewing and either approving or requesting revisions
- Multiple revision cycles if Editor requests changes

---

## Understanding Key Concepts

### Agent Orchestration
**Orchestration** means coordinating multiple agents to work together. In this lab:
- Agents have specialized roles (research, write, edit)
- They communicate through a shared chat history
- Custom strategies control the flow and completion

### Function Calling / Tool Use
The Researcher agent can **call functions** (tools) to search the web. The LLM:
1. Recognizes when it needs more information
2. Calls the appropriate search tool
3. Receives results
4. Uses them to complete its task

### Sequential vs. Autonomous Agent Patterns

**Sequential** (what we built):
- Agents run in a predetermined order
- Each agent completes before the next starts
- Predictable, easy to debug

**Autonomous** (alternative):
- Agents decide when to participate
- Can run in parallel or reactively
- More flexible but harder to control

---

## Challenges and Extensions

Once you have the basic system working, try these enhancements:

### Challenge 1: Add a Fact Checker Agent
Create a fourth agent that verifies claims before the Editor reviews.

### Challenge 2: Implement Parallel Research
Modify the Researcher to explore multiple subtopics simultaneously.

### Challenge 3: Add Memory
Store previous research sessions and allow agents to reference past work.

### Challenge 4: Create a Web UI
Build a web interface to visualize the agent collaboration in real-time.

### Challenge 5: Custom Termination Criteria
Modify the termination strategy to also check for quality metrics or word count.

---

## Troubleshooting

### "GITHUB_TOKEN not found"
Make sure you've set your environment variable:
```powershell
$env:GITHUB_TOKEN="your-token-here"
```

### "TAVILY_API_KEY not found"
Get your free API key from https://app.tavily.com/ and set it:
```powershell
$env:TAVILY_API_KEY="your-key-here"
```

### Agents not using tools
Verify that:
- `FunctionChoiceBehavior.Auto()` is set on the Researcher agent
- MCP connection is successful (check console output)
- The agent's template includes instructions to use search tools

### Infinite loops
If the workflow doesn't terminate:
- Check that Editor template includes approval/revision keywords
- Verify `MaximumIterations` is set
- Review the termination strategy logic

---

## Summary

Congratulations! You've built a sophisticated multi-agent content creation system that:

✅ Orchestrates three specialized AI agents  
✅ Integrates external tools via MCP  
✅ Implements custom agent selection logic  
✅ Handles iterative revision workflows  
✅ Provides clear visibility into agent collaboration  

This pattern can be adapted for many use cases:
- Customer service systems with specialized agents
- Research and analysis pipelines
- Code review and generation workflows
- Content creation and editing systems

The key takeaway: **Breaking complex tasks into specialized agents with clear hand-offs creates more reliable and maintainable AI systems.**

---

## Additional Resources

- [Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel/)
- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [GitHub Models Documentation](https://github.com/marketplace/models)
- [Tavily Search API](https://tavily.com/)
