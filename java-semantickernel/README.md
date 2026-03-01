# Java Semantic Kernel Agent

A command-line AI agent built with Java and Microsoft Semantic Kernel that demonstrates the power of AI function calling through modular plugins. This educational project shows how AI models can intelligently invoke custom functions to perform tasks they couldn't accomplish on their own, such as precise calculations, real-time data access, and string manipulation.

## Getting Started

For complete setup instructions, prerequisites, and a step-by-step lab guide, see [INSTRUCTIONS.md](INSTRUCTIONS.md).

## About This Project

This application showcases how to build AI agents that extend language model capabilities through **function calling** (also known as tool use). Instead of the AI hallucinating answers or approximating results, it can invoke specialized functions to get accurate, real-time information.

## About This Project

This application showcases how to build AI agents that extend language model capabilities through **function calling** (also known as tool use). Instead of the AI hallucinating answers or approximating results, it can invoke specialized functions to get accurate, real-time information.

### What is Semantic Kernel?

**Semantic Kernel** is Microsoft's lightweight, open-source SDK that enables developers to integrate AI into their applications. It provides:
- **Kernel**: The orchestration engine that manages AI services and plugins
- **Plugins**: Collections of functions the AI can discover and invoke
- **Connectors**: Integrations with various AI services (OpenAI, Azure OpenAI, Hugging Face, etc.)
- **Planners**: Automatic multi-step task planning (optional, not used in this starter)

Think of it as a framework that makes AI models "programmable" - you define what the AI can do, and it figures out when and how to do it.

### What are Plugins?

Plugins are collections of functions that extend the AI's capabilities. Each function:
- Is annotated with `@DefineKernelFunction` so the kernel can discover it
- Has a description explaining what it does and when to use it
- Includes parameter descriptions so the AI knows what to pass
- Returns results that the AI incorporates into its response

**Example:** When you ask "What time is it?", the AI:
1. Recognizes it needs current time information
2. Finds the `getCurrentTime` function in TimePlugin
3. Invokes the function to get real system time
4. Incorporates the result into a natural language response

This is fundamentally different from the AI guessing or using outdated training data.

### How Function Calling Works

1. **User Query**: "What is 25 * 4 + 10?"
2. **AI Analysis**: The model recognizes this requires calculation
3. **Function Discovery**: Finds `MathPlugin.calculate()` matches the need
4. **Function Invocation**: Kernel executes `calculate("25 * 4 + 10")`
5. **Result Integration**: AI receives "110" and responds naturally
6. **User Response**: "The answer is 110"

The AI doesn't perform the calculation - it orchestrates the right tool for the job.

## Features

This project demonstrates:
- **Semantic Kernel Integration**: Microsoft's AI orchestration SDK for Java
- **GitHub Models**: Access to OpenAI's GPT-4o model via GitHub's free tier
- **Plugin Architecture**: Three example plugins showing different use cases:
  - **TimePlugin**: Real-time system data access
  - **MathPlugin**: Precise calculations using JavaScript ScriptEngine
  - **StringPlugin**: Text manipulation operations
- **Automatic Function Calling**: The kernel intelligently selects and invokes functions
- **Type Safety**: Strong typing with Java annotations and compile-time checks
- **Error Handling**: Graceful degradation with informative error messages
- **Environment Configuration**: Secure token management via `.env` files

## How It Works

The application creates a kernel configured with:
1. **AI Service**: OpenAI chat completion via GitHub Models endpoint
2. **Plugins**: Registered collections of callable functions
3. **Tool Call Behavior**: Automatic function invocation enabled
4. **Chat History**: Maintains conversation context

When you send a query, the kernel:
- Analyzes what the user wants
- Determines if any registered functions can help
- Invokes those functions with appropriate parameters
- Synthesizes results into a natural language response

## Project Structure

```
java-semantickernel/
├── src/
│   └── main/
│       └── java/
│           └── com/
│               └── codeyou/
│                   └── agent/
│                       ├── App.java           # Main application and kernel setup
│                       ├── TimePlugin.java    # System time access functions
│                       ├── MathPlugin.java    # Mathematical expression evaluator
│                       └── StringPlugin.java  # String manipulation utilities
├── pom.xml                                    # Maven dependencies and build config
├── .env                                       # Environment variables (git-ignored)
├── INSTRUCTIONS.md                            # Complete lab guide
└── README.md                                  # This file
```

## Plugin Examples

### TimePlugin
Provides access to real-time system information:
```java
@DefineKernelFunction(
    name = "getCurrentTime",
    description = "Gets the current date and time"
)
public String getCurrentTime() {
    return LocalDateTime.now()
        .format(DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss"));
}
```

### MathPlugin
Evaluates mathematical expressions with precision:
```java
@DefineKernelFunction(
    name = "calculate",
    description = "Evaluates a mathematical expression"
)
public String calculate(
    @KernelFunctionParameter(
        name = "expression",
        description = "The mathematical expression to evaluate"
    ) String expression) {
    // Uses ScriptEngine to safely evaluate expressions
    return result;
}
```

### StringPlugin
Performs text transformations:
```java
@DefineKernelFunction(
    name = "reverseString",
    description = "Reverses a string"
)
public String reverseString(
    @KernelFunctionParameter(
        name = "input",
        description = "The string to reverse"
    ) String input) {
    return new StringBuilder(input).reverse().toString();
}
```

## Example Queries and Behavior

| Query | Plugin Used | Behavior |
|-------|-------------|----------|
| "What time is it?" | TimePlugin | Gets actual system time, not training data |
| "Calculate 25 * 4 + 10" | MathPlugin | Returns precise result: 110 |
| "Reverse 'Hello World'" | StringPlugin | Returns: "dlroW olleH" |
| "What's the weather?" | None | AI responds with limitation (no weather plugin) |

## Customization

### Adding New Plugins

Plugins extend what the AI can do. To create a new plugin:

1. **Create a plugin class** with annotated methods:

```java
package com.codeyou.agent;

import com.microsoft.semantickernel.semanticfunctions.annotations.DefineKernelFunction;
import com.microsoft.semantickernel.semanticfunctions.annotations.KernelFunctionParameter;

public class WeatherPlugin {
    
    @DefineKernelFunction(
        name = "getWeather",
        description = "Gets weather information for a specific date",
        returnDescription = "Weather description with temperature"
    )
    public String getWeather(
            @KernelFunctionParameter(
                name = "date",
                description = "The date in yyyy-MM-dd format"
            ) String date) {
        // Your logic here - could call a weather API
        return "Sunny, 72°F";
    }
}
```

2. **Register it with the kernel** in `App.java`:
```java
KernelPlugin weatherPlugin = KernelPluginFactory.createFromObject(
    new WeatherPlugin(), 
    "WeatherPlugin"
);
kernel = kernelBuilder.withPlugin(weatherPlugin).build();
```

3. **The AI automatically discovers it** - no prompt engineering needed!

### Changing AI Models

The application uses GitHub Models for free access to OpenAI models. You can switch models by changing the `modelId`:

```java
ChatCompletionService chatCompletionService = 
    OpenAIChatCompletion.builder()
        .withOpenAIAsyncClient(openAIAsyncClient)
        .withModelId("openai/gpt-4o")  // Try: gpt-4o-mini, gpt-4-turbo
        .build();
```

Available models: https://github.com/marketplace/models

## Technology Stack

### Core Dependencies

- **semantic-kernel-api** (1.x): Core Semantic Kernel framework providing plugin architecture and orchestration
- **semantic-kernel-connectors-ai-openai** (1.x): OpenAI integration for chat completion services
- **azure-ai-openai** (1.x): Azure OpenAI SDK (used for GitHub Models endpoint compatibility)
- **slf4j-simple** (2.x): Simple logging facade for debugging and monitoring
- **dotenv-java** (3.x): Environment variable management for secure token storage

### Build Tools

- **Java 17+**: Required for modern language features and Semantic Kernel compatibility
- **Maven 3.6+**: Dependency management and build automation

## Key Concepts

### Function Calling vs. Prompt Engineering

**Without Function Calling (Unreliable):**
```
User: "What is 25 * 4 + 10?"
AI: "I believe the answer is approximately 110" ← May be wrong
```

**With Function Calling (Accurate):**
```
User: "What is 25 * 4 + 10?"
AI: [Invokes MathPlugin.calculate("25 * 4 + 10")]
Plugin: Returns "110"
AI: "The answer is 110" ← Always correct
```

### Automatic Tool Selection

The AI model itself decides when to use functions - you don't need to:
- Parse user intent manually
- Write if/else logic for different query types
- Specify which function to call

The kernel handles this through:
1. **Function descriptions**: AI reads `@DefineKernelFunction` annotations
2. **Parameter schemas**: AI understands what inputs are needed
3. **Automatic invocation**: Kernel executes functions and returns results
4. **Response synthesis**: AI incorporates results naturally

### Multi-Function Chaining

The AI can chain multiple functions in one query:

```
Query: "What's the weather like today?"
Step 1: Call TimePlugin.getCurrentTime() → "2026-01-13"
Step 2: Call WeatherPlugin.getWeather("2026-01-13") → "Sunny, 72°F"
Response: "Today's weather is sunny with a temperature of 72°F"
```

## Learning Resources

- **Semantic Kernel Documentation**: https://learn.microsoft.com/semantic-kernel
- **GitHub Models**: https://github.com/marketplace/models
- **OpenAI Function Calling Guide**: https://platform.openai.com/docs/guides/function-calling
- **Lab Instructions**: [INSTRUCTIONS.md](INSTRUCTIONS.md)

## Educational Goals

This project teaches:
1. **AI Orchestration**: How to coordinate LLMs with custom code
2. **Plugin Architecture**: Building modular, reusable AI capabilities
3. **Function Calling**: The difference between AI assistance and AI agents
4. **Practical Integration**: Real-world patterns for AI-powered applications

## License

This is an educational project for learning AI agent development with Semantic Kernel.

---

**Ready to get started?** See [INSTRUCTIONS.md](INSTRUCTIONS.md) for the complete step-by-step lab guide.
