# Java Semantic Kernel AI Agent - Student Lab Instructions

## Prerequisites

Before starting this lab, ensure you have the following installed:

1. **Java Development Kit (JDK) 17 or higher**
   - Download from: https://www.oracle.com/java/technologies/downloads/
   - Verify installation: `java -version`

2. **Apache Maven**
   - Download from: https://maven.apache.org/download.cgi
   - Verify installation: `mvn -version`

3. **Visual Studio Code**
   - Install the "Extension Pack for Java" from the VS Code marketplace
   - Install the "GitHub Copilot" extension

4. **GitHub Models Access**
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

5. **Create .gitignore File**
   - Download the Java .gitignore template from GitHub:
     - Visit: https://github.com/github/gitignore/blob/main/Java.gitignore
     - Click "Raw" button and save the content to a `.gitignore` file in your project root
     - Or use this direct link: https://raw.githubusercontent.com/github/gitignore/main/Java.gitignore
   - Additionally, add `.env` to your .gitignore file if it's not already included
   - This prevents committing sensitive data (like your GitHub token), build artifacts, and IDE files to version control

6. **Create .env File**
   - Create a `.env` file in the project root directory (this should already be in your .gitignore)
   - Add your GitHub token: `GITHUB_TOKEN=your_token_here`
   - Replace `your_token_here` with your actual GitHub Personal Access Token
   - **Important**: Never commit this file to version control - verify it's listed in `.gitignore`

---

## 💡 Working with GitHub Copilot Chat for Debugging

**When you encounter runtime errors:**

1. **Copy the error message** from your terminal or console output
2. **Open GitHub Copilot Chat** (click the chat icon in VS Code sidebar or use `Ctrl+Alt+I` / `Cmd+Alt+I`)
3. **Provide context** by typing something like:
   ```
   I'm getting this error when running my Java Semantic Kernel application:
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

## Lab Exercise: Building an AI Agent with Java and Semantic Kernel

### Part 1: Project Setup

**Prompt 1: Create Maven Project**
```
Create a new Maven project for a Java Semantic Kernel AI agent application. The project should:
- Use Java 17
- Include dependencies for Microsoft Semantic Kernel, Azure OpenAI client library, and dotenv-java
- Have groupId: com.codeyou
- Have artifactId: java-semantickernel
- Have version: 1.0-SNAPSHOT
- Create the pom.xml file
```

**Prompt 2: Restore Dependencies**
After creating the project file, restore Maven dependencies:
```bash
mvn clean install
```
Verify the following dependencies were installed:
- Microsoft Semantic Kernel
- Azure OpenAI client library
- dotenv-java
```

Verify that AI ran the mvn clean install command and that the dependencies were installed.

**Prompt 3: Create Project Structure**
```
Create the standard Maven directory structure and a basic App.java main class in src/main/java/com/codeyou/agent/
```

**Prompt 4: Create VS Code Configuration**
```
Create VS Code tasks.json and launch.json for the Maven console application in the java-semantickernel folder.
```

---

### Part 2: Basic Application Setup (Without Function Calling)

**Prompt 5: Load Environment Variables**
```
In App.java, add code to:
- Load environment variables from a .env file using dotenv-java
- Get the GITHUB_TOKEN from environment variables
- Display an error message if the token is not found
- Include helpful user feedback with emoji
```

**Prompt 6: Initialize GitHub Models Client**
```
Add code to create an OpenAI async client using the Azure OpenAI SDK configured to use GitHub Models endpoint (https://models.github.ai/inference) with the GitHub token from environment variables
```

**Prompt 7: Create Chat Completion Service**
```
Create an OpenAI chat completion service using Semantic Kernel that uses the model "openai/gpt-4o" (available on GitHub Models)
```

**Prompt 8: Build Basic Kernel**
```
Create a Semantic Kernel instance and register the chat completion service with it
```

**Prompt 9: Test Math Query (Without Function)**
```
Add code to:
- Get the chat completion service from the kernel
- Create a ChatHistory object
- Add a user message asking "What is 25 * 4 + 10?"
- Get a response from the AI without any function calling
- Print the result
- Note: The AI will try to answer on its own without tools
```

**Test Point**: Run the application using `mvn exec:java`. You should see the AI attempt to answer the math question, but it may not be accurate since it doesn't have access to calculation tools.

---

### Part 3: Adding Function Calling with Plugins

**Prompt 10: Create MathPlugin**
```
Create a MathPlugin.java class that:
- Has a calculate method annotated with @DefineKernelFunction
- Takes a mathematical expression as a string parameter
- Uses JavaScript ScriptEngine to evaluate the expression
- Returns the result as a string
- Includes error handling
- Includes proper descriptions for the function and parameter
```

**Prompt 11: Register MathPlugin with Kernel**
```
Update App.java to:
- Create an instance of MathPlugin using KernelPluginFactory
- Register the plugin with the kernel builder
```

**Prompt 12: Add Function Calling Support**
```
Update the chat interaction code to:
- Create an InvocationContext with ToolCallBehavior that allows all kernel functions
- Use getChatMessageContentsAsync with the kernel and invocation context
- Use .block() to wait for the async result
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
Create a StringPlugin.java class that:
- Has a reverseString method annotated with @DefineKernelFunction
- Takes a string input parameter
- Returns the reversed string
- Includes proper descriptions for the function and parameter
```

**Prompt 15: Register StringPlugin with Kernel**
```
Update App.java to:
- Create an instance of StringPlugin using KernelPluginFactory
- Register the StringPlugin with the kernel builder (along with MathPlugin)
```

**Test Point**: Run the application again. Now the AI should use the StringPlugin to accurately reverse the string and return "dlroW olleH".

---

**Prompt 16: Test Time Query (Without Function)**
```
Replace the string query with a new query: "What time is it right now?"
Comment out the TimePlugin registration (keep Math and String plugins commented)
Run the application and observe that the AI cannot provide the current time
```

**Test Point**: Run the application. The AI will not know the current time since it doesn't have access to system time.

---

**Prompt 17: Create TimePlugin**
```
Create a new TimePlugin.java class in the same package that:
- Has a getCurrentTime method annotated with @DefineKernelFunction
- Returns the current date and time formatted as "yyyy-MM-dd HH:mm:ss"
- Includes proper descriptions for the function
```

**Prompt 18: Register TimePlugin with Kernel**
```
Update App.java to:
- Create an instance of TimePlugin using KernelPluginFactory
- Register all three plugins (TimePlugin, MathPlugin, and StringPlugin) with the kernel builder
```

**Prompt 19: Create Multiple Test Queries**
```
Replace the single query with an array of test queries:
- "What time is it right now?"
- "What is 25 * 4 + 10?"
- "Reverse the string 'Hello World'"

Loop through each query, add it to chat history, get the AI response with function calling, and display results with clear formatting
```

**Test Point**: Run the application. All three queries should now work accurately using their respective plugins.

---

**Prompt 20: Add System Prompt**
```
Update the chat history creation to include a system message that instructs the AI to be professional and succinct. Add the system message immediately after creating the ChatHistory object and before adding any user messages.
```

**Prompt 21: Add Error Handling**
```
Wrap the query loop and individual queries in try-catch blocks to handle any errors gracefully and display user-friendly error messages
```

---

### Part 4: Testing

**Testing Instructions:**

Run the application with all three plugins enabled using `mvn exec:java`. You should observe:
- Time queries return accurate current time via TimePlugin
- Math calculations are precise via MathPlugin
- String reversal is correct via StringPlugin
- The AI seamlessly chooses and uses the appropriate function for each query

---

## Expected Output

When running with function calling enabled, you should see output similar to:

```
🤖 Java Semantic Kernel Agent Starting...

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
   - Has a @DefineKernelFunction annotation
   - Accepts a date parameter (formatted as "yyyy-MM-dd")
   - Returns "Sunny, 72°F" if the date matches today's date
   - Returns "Rainy, 55°F" for all other dates
   - Include proper descriptions for both the function and parameter

2. **Register the plugin** with your kernel along with the existing TimePlugin

3. **Test with a query** that requires two function calls:
   - Ask the AI: "What's the weather like today?"
   - The AI should:
     - First call TimePlugin.getCurrentTime() to get today's date
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
3. Add comprehensive logging throughout the application
   - Log all AI requests and responses
   - Log function calls with parameters and results
   - Use a logging framework like SLF4J with Logback
   - Include timestamps and log levels (INFO, DEBUG, ERROR)
   - Ask Copilot: "Add logging to track AI interactions and function calls using SLF4J"

**Performance Monitoring**
4. Add performance metrics and timing
   - Measure response time for each AI query
   - Track function execution duration
   - Log slow queries (over a threshold)
   - Ask Copilot: "Add performance monitoring to track query response times"

**Error Handling & Resilience**
5. Implement robust error handling
   - Add retry logic with exponential backoff for API failures
   - Handle rate limiting scenarios gracefully
   - Provide detailed error messages to users
   - Ask Copilot: "Add retry logic with exponential backoff for AI API calls"

**Input Validation**
6. Add input validation and sanitization
   - Validate user queries before sending to AI
   - Sanitize inputs to prevent injection attacks
   - Set maximum query length limits
   - Ask Copilot: "Add input validation to sanitize and validate user queries"

**Handling Rate Limit Responses (HTTP 429)**
7. Handle rate limiting responses from the API
   - Detect and catch HTTP 429 (Too Many Requests) errors
   - Parse retry-after headers from the response
   - Implement automatic retry after the specified delay
   - Display user-friendly messages when rate limited
   - Ask Copilot: "Add handling for HTTP 429 rate limit responses with automatic retry"

---

## Troubleshooting

### Maven Build Issues

If you encounter dependency resolution issues, try:
```bash
mvn clean install -U
```

### Java Version Issues

Ensure you're using Java 17 or later. Check with:
```bash
java -version
```

Set JAVA_HOME if needed:
```powershell
# Windows PowerShell
$env:JAVA_HOME="C:\Program Files\Java\jdk-17"

# Linux/Mac
export JAVA_HOME=/path/to/jdk-17
```

### Running the Application

**Using Maven:**
```bash
mvn exec:java
```

**Using the compiled JAR:**
```bash
java -jar target/java-semantickernel-1.0-SNAPSHOT.jar
```

### Environment Variable Issues

If the application can't find your GITHUB_TOKEN:
1. Verify the `.env` file exists in the project root directory
2. Check that the file contains: `GITHUB_TOKEN=your_token_here`
3. Make sure there are no extra spaces or quotes around the token value
4. Restart your terminal/IDE after creating the `.env` file
