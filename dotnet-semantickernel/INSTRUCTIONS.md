# C# Semantic Kernel AI Agent - Student Lab Instructions

## Prerequisites

Before starting this lab, ensure you have the following installed:

1. **.NET SDK 8.0 or higher**
   - Download from: https://dotnet.microsoft.com/download
   - Verify installation: `dotnet --version`

2. **Visual Studio Code**
   - Install the "C# Dev Kit" extension from the VS Code marketplace
   - Install the "GitHub Copilot" extension

3. **GitHub Models Access**
   - Sign in to GitHub with your account
   - Visit https://github.com/marketplace/models
   - Find and select the **gpt-4o** model from OpenAI
   - Click "Get started" or "Deploy" to enable the model for your account
   - Create a Personal Access Token (PAT) with the following steps:
     1. **Navigate to Developer Settings**: In GitHub, click your profile photo, go to Settings, then click Developer settings in the left sidebar
     2. **Select Fine-grained Tokens**: Under "Personal access tokens", select Fine-grained tokens and then click Generate new token
     3. **Configure Token Details**: Give the token a descriptive name (e.g., "GitHub Models Access") and set an expiration period (recommended for security)
     4. **Select Repository Access**: Choose whether the token can access all repositories or only specific ones. For security, it is best to select "Only select repositories" and choose the minimal number needed
     5. **Add Permissions**: Under the "Permissions" section, find the **Models** permission under "Account permissions" and set its access level to **Read**
     6. **Generate and Save**: Click Generate token at the bottom of the page. Immediately copy the token and store it in a secure location, as you will not be able to see it again
   - Or use your GitHub Copilot subscription which includes access to GitHub Models

4. **Create .gitignore File**
   - Download the Visual Studio .gitignore template from GitHub:
     - Visit: https://github.com/github/gitignore/blob/main/VisualStudio.gitignore
     - Click "Raw" button and save the content to a `.gitignore` file in your project root
     - Or use this direct link: https://raw.githubusercontent.com/github/gitignore/main/VisualStudio.gitignore
   - Additionally, add `.env` to your .gitignore file if it's not already included
   - This prevents committing build artifacts, IDE files, and sensitive data to version control

5. **Set Up GitHub Token**
   
   You'll need to set your GitHub token using one of these methods:

   **Option A: User Secrets (recommended for development)**
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "GITHUB_TOKEN" "your-github-token-here"
   ```
   This is the most secure option for local development. The token is stored outside your project directory and won't be committed to version control.

   **Option B: Environment Variable (for current session)**
   ```powershell
   $env:GITHUB_TOKEN="your-github-token-here"
   ```
   This sets the token for the current terminal session only. You'll need to set it again if you close the terminal
   This stores the token securely and persists across sessions. Recommended for ongoing development work.

   **Where to get your token:**
   - Get your GitHub token from: https://github.com/settings/tokens
   - Or use GitHub Models: https://github.com/marketplace/models
   - Replace `your-github-token-here` with your actual GitHub Personal Access Token

---

## 💡 Working with GitHub Copilot Chat for Debugging

**When you encounter runtime errors:**

1. **Copy the error message** from your terminal or console output
2. **Open GitHub Copilot Chat** (click the chat icon in VS Code sidebar or use `Ctrl+Alt+I` / `Cmd+Alt+I`)
3. **Provide context** by typing something like:
   ```
   I'm getting this error when running my C# Semantic Kernel application:
   [paste your error message here]
   
   Can you help me fix it?
   ```
4. **Review the suggestion** and apply the fix Copilot recommends
5. **Ask follow-up questions** if you don't understand the solution

**Pro Tips:**
- Include the full error stack trace for better diagnostics
- Mention what you were trying to do when the error occurred
- If the first suggestion doesn't work, tell Copilot and ask for alternatives
- Use Copilot Chat to explain error messages you don't understand

---

## Lab Exercise: Building an AI Agent with C# and Semantic Kernel

### Part 1: Project Setup

**Prompt 1: Create .NET Project**
```
Create a new .NET console application for a C# Semantic Kernel AI agent. The project should:
- Use .NET 8.0 or higher
- Include NuGet packages for Microsoft.SemanticKernel, Microsoft.Extensions.Configuration, and Microsoft.Extensions.Configuration.UserSecrets
- Create the .csproj file
- Name the project: dotnet-semantickernel
```

**Prompt 2: Restore Dependencies**
After creating the project file, restore NuGet packages:
dotnet restore
```
Verify the following packages were installed:
- Microsoft.SemanticKernel
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Configuration.UserSecrets
```

Verify that AI ran the dotnet restore command and that the dependencies were installed.

**Prompt 3: Create Project Structure**
```
Create a Program.cs file with a basic Main method using top-level statements and the SemanticKernelAgent namespace
```

**Prompt 4: Create VS Code Configuration**
```
Create VS Code tasks.json and launch.json for the .NET console application in the dotnet-semantickernel folder.
```

---

### Part 2: Basic Application Setup (Without Function Calling)

**Prompt 5: Load Environment Variables**
```
In Program.cs, add code to:
- Load configuration from environment variables and user secrets
- Get the GITHUB_TOKEN from configuration
- Display an error message if the token is not found
- Include helpful user feedback with emoji
- Use ConfigurationBuilder with AddEnvironmentVariables and AddUserSecrets
```

**Prompt 6: Initialize GitHub Models Client**
```
Add code to create a Semantic Kernel builder and configure it to use GitHub Models endpoint (https://models.github.ai/inference) with the GitHub token using AddOpenAIChatCompletion
```

**Prompt 7: Configure Chat Completion Service**
```
Configure the OpenAI chat completion service to use the model "openai/gpt-4o" (available on GitHub Models) with the GitHub Models endpoint
```

**Prompt 8: Build Basic Kernel**
```
Build the Semantic Kernel instance from the builder
```

**Prompt 9: Test Math Query (Without Function)**
```
Add code to:
- Get the chat completion service from the kernel using GetRequiredService
- Create a ChatHistory object
- Add a user message asking "What is 25 * 4 + 10?"
- Get a response from the AI without any function calling
- Print the result
- Note: The AI will try to answer on its own without tools
```

**Test Point**: Run the application using `dotnet run`. You should see the AI attempt to answer the math question, but it may not be accurate since it doesn't have access to calculation tools.

---

### Part 3: Adding Function Calling with Plugins

**Prompt 10: Create MathPlugin**
```
Create a new MathPlugin.cs class that:
- Has a Calculate method with [KernelFunction] attribute
- Has a [Description] attribute explaining what the function does
- Takes a mathematical expression as a string parameter with [Description] attribute
- Evaluates the expression and returns the result as a string
- Includes error handling
- Use NCalc or DataTable.Compute for expression evaluation
```

**Prompt 11: Register MathPlugin with Kernel**
```
Update Program.cs to:
- Register the MathPlugin with the kernel builder using builder.Plugins.AddFromType<MathPlugin>()
- Add this before building the kernel
```

**Prompt 12: Add Function Calling Support**
```
Update the chat interaction code to:
- Create OpenAIPromptExecutionSettings with ToolCallBehavior set to AutoInvokeKernelFunctions
- Pass the execution settings to GetChatMessageContentAsync along with the kernel
- Use await to get the async result
```

**Test Point**: Run the application again. Now the AI should use the MathPlugin to accurately calculate "What is 25 * 4 + 10?" and return 110.

---

**Prompt 13: Test String Query (Without Function)**
```
Replace the math query with a new query: "Reverse the string 'Hello World'"
Comment out the MathPlugin registration
Run the application and observe that the AI attempts to reverse the string without tools (may be inaccurate)
```

**Test Point**: Run the application. The AI will try to reverse the string on its own, which may not be reliable.

---

**Prompt 14: Create StringPlugin**
```
Create a StringPlugin.cs class that:
- Has a ReverseString method with [KernelFunction] attribute
- Has a [Description] attribute explaining what the function does
- Takes a string input parameter with [Description] attribute
- Returns the reversed string
- Use string manipulation methods or LINQ to reverse the string
```

**Prompt 15: Register StringPlugin with Kernel**
```
Update Program.cs to:
- Register the StringPlugin with the kernel builder using builder.Plugins.AddFromType<StringPlugin>()
- Uncomment MathPlugin registration as well
```

**Test Point**: Run the application again. Now the AI should use the StringPlugin to accurately reverse the string and return "dlroW olleH".

---

**Prompt 16: Test Time Query (Without Function)**
```
Replace the string query with a new query: "What time is it right now?"
Comment out all plugin registrations
Run the application and observe that the AI cannot provide the current time
```

**Test Point**: Run the application. The AI will not know the current time since it doesn't have access to system time.

---

**Prompt 17: Create TimePlugin**
```
Create a new TimePlugin.cs class that:
- Has a GetCurrentTime method with [KernelFunction] attribute
- Has a [Description] attribute explaining what the function does
- Returns the current date and time formatted as "yyyy-MM-dd HH:mm:ss"
- Use DateTime.Now and ToString() for formatting
```

**Prompt 18: Register TimePlugin with Kernel**
```
Update Program.cs to:
- Register all three plugins (TimePlugin, MathPlugin, and StringPlugin) with the kernel builder
- Use builder.Plugins.AddFromType<T>() for each plugin
```

**Prompt 19: Create Multiple Test Queries**
```
Replace the single query with an array of test queries:
- "What time is it right now?"
- "What is 25 * 4 + 10?"
- "Reverse the string 'Hello World'"

Loop through each query using a foreach loop, add it to chat history, get the AI response with function calling, and display results with clear formatting including emoji and separator lines
```

**Test Point**: Run the application. All three queries should now work accurately using their respective plugins.

---

**Prompt 20: Add System Prompt**
```
Update the chat history creation to include a system message that instructs the AI to be professional and succinct. Add the system message immediately after creating the ChatHistory object and before adding any user messages.
```

**Prompt 21: Add Error Handling**
```
Wrap the query loop and individual queries in try-catch blocks to handle any exceptions gracefully and display user-friendly error messages with emoji
```

---

### Part 4: Testing

**Testing Instructions:**

Run the application with all three plugins enabled using `dotnet run`. You should observe:
- Time queries return accurate current time via TimePlugin
- Math calculations are precise via MathPlugin
- String reversal is correct via StringPlugin
- The AI seamlessly chooses and uses the appropriate function for each query

---

## Expected Output

When running with function calling enabled, you should see output similar to:

```
🤖 C# Semantic Kernel Agent Starting...

Running example queries:

📝 Query: What time is it right now?
──────────────────────────────────────────────────

✅ Result: The current date and time is 2026-01-12 14:30:45

📝 Query: What is 25 * 4 + 10?
──────────────────────────────────────────────────

✅ Result: 110

📝 Query: Reverse the string 'Hello World'
──────────────────────────────────────────────────

✅ Result: dlroW olleH

🎉 Agent demo complete!
```

---

## Final Assessment

Test your understanding by completing this assessment:

### Weather Plugin with Multi-Function Calling

**Objective:** Create a WeatherPlugin that demonstrates the AI's ability to chain multiple function calls together.

**Requirements:**

1. **Create a WeatherPlugin** with a method that:
   - Has a [KernelFunction] attribute
   - Accepts a date parameter (formatted as "yyyy-MM-dd")
   - Returns "Sunny, 72°F" if the date matches today's date
   - Returns "Rainy, 55°F" for all other dates
   - Include a [Description] for both the function and parameter

2. **Register the plugin** with your kernel along with the existing TimePlugin

3. **Test with a query** that requires two function calls:
   - Ask the AI: "What's the weather like today?"
   - The AI should:
     - First call TimePlugin.GetCurrentTime() to get today's date
     - Then call WeatherPlugin with that date to get the weather
     - Return a complete answer combining both pieces of information

**Success Criteria:**
- The AI successfully chains two function calls without explicit instruction
- Mock weather data is returned based on the current date
- The response is coherent and answers the original question

---

## Discussion Questions

After completing the lab, consider:

1. What's the difference between responses with and without function calling?
2. Why are plugins/tools important for AI agents?
3. How does Semantic Kernel orchestrate the function calls?
4. What other types of plugins would be useful for an AI agent?

---

## Extension Challenges

If you finish early, try:

### Additional Plugins
1. Add a FilePlugin that can read/write text files
2. Create a DatabasePlugin that queries a simple in-memory database

### Cross-Cutting Concerns (Use Copilot to Help!)

**Logging & Observability**
4. Add comprehensive logging throughout the application
   - Log all AI requests and responses
   - Log function calls with parameters and results
   - Use Microsoft.Extensions.Logging with ILogger
   - Include timestamps and log levels (Information, Debug, Error)
   - Ask Copilot: "Add logging to track AI interactions and function calls using ILogger"

**Performance Monitoring**
6. Add performance metrics and timing
   - Measure response time for each AI query
   - Track function execution duration
   - Log slow queries (over a threshold)
   - Ask Copilot: "Add performance monitoring to track query response times using Stopwatch"

**Error Handling & Resilience**
6. Implement robust error handling
   - Add retry logic with exponential backoff for API failures
   - Handle rate limiting scenarios gracefully
   - Provide detailed error messages to users
   - Ask Copilot: "Add retry logic with exponential backoff for AI API calls using Polly"

**Input Validation**
7. Add input validation and sanitization
   - Validate user queries before sending to AI
   - Sanitize inputs to prevent injection attacks
   - Set maximum query length limits
   - Ask Copilot: "Add input validation to sanitize and validate user queries"

**Handling Rate Limit Responses (HTTP 429)**
8. Handle rate limiting responses from the API
   - Detect and catch HttpRequestException with status code 429
   - Parse retry-after headers from the response
   - Implement automatic retry after the specified delay
   - Display user-friendly messages when rate limited
   - Ask Copilot: "Add handling for HTTP 429 rate limit responses with automatic retry"
