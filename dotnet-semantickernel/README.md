# C# Semantic Kernel AI Agent - Reference Implementation

A reference implementation of a command-line AI agent built with C# and Microsoft Semantic Kernel. This project demonstrates how to build AI agents with function calling capabilities using GitHub Models.

## About This Project

This is a **reference implementation** for educational purposes. It shows the final working version of an AI agent that students build from scratch following the step-by-step instructions in [INSTRUCTIONS.md](INSTRUCTIONS.md).

**For Students:** Don't copy this code! Follow the [INSTRUCTIONS.md](INSTRUCTIONS.md) file to build your own version using GitHub Copilot. This code is here to show you what you're working toward and to help if you get stuck.

**For Instructors:** Use this as a reference for grading and troubleshooting student implementations.

## What It Does

This AI agent demonstrates:
## What It Does

This AI agent demonstrates:
- **Function Calling with AI** - The AI intelligently selects and uses tools to answer queries
- **Three Custom Plugins** - Math calculation, string reversal, and time retrieval
- **GitHub Models Integration** - Uses OpenAI's GPT-4o model via GitHub's inference endpoint
- **Semantic Kernel Framework** - Microsoft's lightweight SDK for AI orchestration
- **Automatic Tool Selection** - The kernel automatically invokes the appropriate plugin based on user queries

## Architecture

### Semantic Kernel

Microsoft Semantic Kernel is a lightweight SDK that integrates AI into applications. Key concepts:

**Plugins**: Collections of functions that the AI can call. In this project:
- `MathPlugin` - Evaluates mathematical expressions using DataTable.Compute
- `StringPlugin` - Reverses strings using LINQ
- `TimePlugin` - Returns current date/time

**Kernel**: The orchestration engine that:
- Manages the LLM connection (GitHub Models/GPT-4o)
- Registers and invokes plugins
- Handles the conversation flow

**Function Calling**: The AI doesn't just generate text - it can call functions to:
- Get real-time information (current time)
- Perform calculations accurately
- Execute code operations (string manipulation)

### How It Works

1. **User Query** - User asks a question (e.g., "What is 25 * 4 + 10?")
2. **AI Reasoning** - GPT-4o analyzes the query and determines it needs the Calculator
3. **Tool Selection** - Semantic Kernel identifies the MathPlugin.Calculate function
4. **Function Execution** - The Calculate method runs with the expression
5. **Result Integration** - The AI incorporates the result into its response
6. **User Response** - "110" is returned to the user

## Code Structure

```
dotnet-semantickernel/
├── Program.cs           # Main application with kernel setup and query loop
├── MathPlugin.cs        # Calculator function using DataTable.Compute
├── StringPlugin.cs      # String reversal function using LINQ
├── TimePlugin.cs        # Current time retrieval function
└── *.csproj            # Project configuration with NuGet dependencies
```

### Plugin Example

Plugins are simple C# classes with attributed methods:

```csharp
public class MathPlugin
{
    [KernelFunction, Description("Evaluates mathematical expressions")]
    public string Calculate(
        [Description("Mathematical expression to evaluate")] string expression)
    {
        // Evaluation logic
        return result;
    }
}
```

The `[KernelFunction]` and `[Description]` attributes tell the AI:
- This function is available to call
- When to use it (based on the description)
- What parameters it expects

## Running the Sample

If you want to run this reference implementation:

1. Ensure you have .NET 8.0+ installed
2. Set your GitHub token:
   ```powershell
   $env:GITHUB_TOKEN="your-github-token-here"
   ```
3. Run:
   ```bash
   dotnet restore
   dotnet run
   ```

**Note:** For detailed setup instructions and the learning experience, see [INSTRUCTIONS.md](INSTRUCTIONS.md).

## Example Output

```
🤖 C# Semantic Kernel Agent Starting...

Running example queries:

📝 Query: What time is it right now?
──────────────────────────────────────────────────
✅ Result: The current date and time is 2026-01-13 14:30:45

📝 Query: What is 25 * 4 + 10?
──────────────────────────────────────────────────
✅ Result: 110

📝 Query: Reverse the string 'Hello World'
──────────────────────────────────────────────────
✅ Result: dlroW olleH

🎉 Agent demo complete!
```

## Key Concepts Demonstrated

### 1. Auto Function Calling
The AI autonomously decides when to use tools without explicit programming logic for each case.

### 2. Plugin System
Modular, reusable functions that extend AI capabilities beyond text generation.

### 3. Type Safety
C# attributes provide strong typing and compile-time validation of plugin definitions.

### 4. Configuration Management
Uses .NET's configuration system for secure token management (user secrets).

### 5. Error Handling
Graceful handling of API failures, missing tokens, and plugin execution errors.

## Learning Objectives

This reference implementation teaches:
- How AI agents differ from simple chatbots
- The value of function calling vs. pure LLM responses
- How to structure plugins for optimal AI understanding
- Integration patterns with GitHub Models
- C# best practices for AI applications

## Extending This Project

Common extensions (detailed in [INSTRUCTIONS.md](INSTRUCTIONS.md) extension challenges):
- Add more plugins (Weather, File I/O, Database)
- Implement logging and observability
- Add performance monitoring
- Build retry logic and resilience
- Create conversation memory for multi-turn dialogues

## Technologies Used

- **Microsoft.SemanticKernel** - Core AI orchestration framework
- **Microsoft.Extensions.Configuration** - Configuration management
- **Microsoft.Extensions.Configuration.UserSecrets** - Secure token storage
- **.NET 8.0** - Runtime and SDK
- **GitHub Models API** - AI inference endpoint (OpenAI GPT-4o)

## Related Files

- **[INSTRUCTIONS.md](INSTRUCTIONS.md)** - Step-by-step guide to build this from scratch
- **Program.cs** - Main application logic and kernel configuration
- **MathPlugin.cs, StringPlugin.cs, TimePlugin.cs** - Plugin implementations

## Resources

- [Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel/)
- [GitHub Models](https://github.com/marketplace/models)
- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/)

---

**This is a reference implementation for educational purposes. To learn by building, see [INSTRUCTIONS.md](INSTRUCTIONS.md).**
