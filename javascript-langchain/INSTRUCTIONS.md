# JavaScript LangChain AI Agent - Student Lab Instructions

## Prerequisites

Before starting this lab, ensure you have the following installed:

1. **Node.js (Version 18 or higher)**
   - Download from: https://nodejs.org/
   - Verify installation: `node --version`
   - Verify npm: `npm --version`

2. **Visual Studio Code**
   - Install the "JavaScript and TypeScript" extension (usually pre-installed)
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
   - Download the Node.js .gitignore template from GitHub:
     - Visit: https://github.com/github/gitignore/blob/main/Node.gitignore
     - Click "Raw" button and save the content to a `.gitignore` file in your project root
     - Or use this direct link: https://raw.githubusercontent.com/github/gitignore/main/Node.gitignore
   - **Important**: Verify that `.env` is included in the .gitignore (it should be by default)
   - This prevents committing sensitive data (like your GitHub token) and dependencies to version control

5. **Create .env File**
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
   I'm getting this error when running my JavaScript LangChain application:
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

## Lab Exercise: Building an AI Agent with JavaScript and LangChain

### Part 1: Project Setup

**Prompt 1: Initialize Node.js Project**
```
Initialize a new Node.js project for a JavaScript LangChain AI agent application. Create a package.json file with:
- Name: javascript-langchain
- Version: 1.0.0
- Type: module (for ES6 imports)
- Description: AI Agent using LangChain and GitHub Models
- Main entry point: app.js
```

**Prompt 2: Install Dependencies**
```
Add npm install commands to install the following packages:
- @langchain/openai (for OpenAI integration)
- @langchain/community (for community tools like Calculator)
- @langchain/core (for core LangChain utilities)
- langchain (main LangChain library)
- dotenv (for environment variables)
```

Verify that AI ran the npm install command and that the dependencies were installed.

**Prompt 3: Create Basic App Structure**
```
Create an app.js file that:
- Uses ES6 module imports
- Has an async main() function
- Calls main().catch(console.error) at the end
- Includes a starting message with emoji
```

**Prompt 4: Create VS Code Configuration**
```
Create VS Code tasks.json and launch.json for the node console application in the javascript-langchain folder.
```

---

### Part 2: Basic Application Setup (Without Tools)

**Prompt 5: Load Environment Variables**
```
In app.js, add code to:
- Import and configure dotenv
- Check if GITHUB_TOKEN exists in environment variables
- Display an error message with helpful instructions if the token is not found
- Exit the process if no token is found
- Include helpful user feedback with emoji
```

**Prompt 6: Initialize ChatOpenAI Model**
```
Add code to create a ChatOpenAI instance that:
- Uses the model "openai/gpt-4o"
- Sets temperature to 0 for deterministic responses
- Configures the baseURL to "https://models.github.ai/inference"
- Uses the GITHUB_TOKEN as the apiKey in the configuration
```

**Prompt 7: Test Basic Query (Without Tools)**
```
Add code to:
- Import the ChatOpenAI's invoke method
- Create a test query: "What is 25 * 4 + 10?"
- Call model.invoke() with the query
- Print the response content
- Note: The AI will try to answer on its own without tools
```

**Test Point**: Run the application with `node app.js`. You should see the AI attempt to answer the math question, but it may not be accurate since it doesn't have access to calculation tools.

---

### Part 3: Adding Tools and Agent Executor

**Prompt 8: Import Agent and Tools**
```
Update the imports to include:
- initializeAgentExecutorWithOptions from "langchain/agents"
- Calculator from "@langchain/community/tools/calculator"
- DynamicTool from "@langchain/core/tools"
```

**Prompt 9: Create Calculator Tool**
```
After initializing the model, create a tools array with:
- The Calculator tool from @langchain/community
```

**Prompt 10: Create Agent Executor**
```
Add code to:
- Create an agent executor using initializeAgentExecutorWithOptions
- Pass the tools array, model, and configuration object with agentType: "openai-functions" and verbose: true
- Use await since it's an async function
```

**Prompt 11: Update Query to Use Agent**
```
Replace the direct model.invoke() call with:
- Create a test query: "What is 25 * 4 + 10?"
- Use executor.invoke() with an object containing the input query
- Print the result.output
- Wrap in try-catch for error handling
```

**Test Point**: Run the application again. Now the AI should use the Calculator tool to accurately calculate "What is 25 * 4 + 10?" and return 110. You should see verbose output showing the tool being called.

---

**Prompt 12: Test Time Query (Without Tool)**
```
Replace the math query with a new query: "What time is it right now?"
Comment out the Calculator tool from the tools array
Run the application and observe that the AI cannot provide the current time accurately
```

**Test Point**: Run the application. The AI will not know the current time since it doesn't have access to system time.

---

**Prompt 13: Create Time Tool**
```
Add a DynamicTool to the tools array that:
- Has name: "get_current_time"
- Has a description explaining it returns the current date and time
- Has a func that is an async function returning new Date().toString()
```

**Prompt 14: Test Time Query with Tool**
```
Uncomment or add back the Calculator tool
Keep the query: "What time is it right now?"
Run the application
```

**Test Point**: Run the application. Now the AI should use the get_current_time tool to provide the current time.

---

**Prompt 15: Test String Query (Without Tool)**
```
Replace the query with: "Reverse the string 'Hello World'"
Comment out the time tool
Run the application and observe that the AI attempts to reverse the string but may not be reliable
```

**Test Point**: Run the application. The AI will try to reverse the string on its own, which may not be perfect.

---

**Prompt 16: Create String Reversal Tool**
```
Add a DynamicTool to the tools array that:
- Has name: "reverse_string"
- Has a description: "Reverses a string. Input should be a single string."
- Has a func that is an async function taking input and returning the reversed string using split("").reverse().join("")
```

**Prompt 17: Test with All Three Tools**
```
Ensure all three tools are in the tools array:
- Calculator
- get_current_time
- reverse_string
Update the query to: "Reverse the string 'Hello World'"
```

**Test Point**: Run the application. The AI should use the reverse_string tool to accurately reverse the string and return "dlroW olleH".

---

**Prompt 18: Create Multiple Test Queries**
```
Replace the single query with an array of test queries:
- "What time is it right now?"
- "What is 25 * 4 + 10?"
- "Reverse the string 'Hello World'"

Add a loop that:
- Iterates through each query
- Prints the query with formatting
- Calls executor.invoke() for each query
- Prints the result with formatting
- Includes try-catch error handling for each query
```

**Test Point**: Run the application. All three queries should now work accurately using their respective tools. You should see verbose output showing which tool is being called for each query.

---

**Prompt 19: Improve Output Formatting**
```
Update the output formatting to:
- Print a header "Running example queries:"
- For each query, print a separator line using "─".repeat(50)
- Display results with ✅ emoji for success
- Display errors with ❌ emoji for failures
- Add a completion message at the end
```

---

**Prompt 20: Add System Message**
```
Update the agent executor initialization to include a system message that instructs the AI to be professional and succinct. Add this configuration before creating the agent executor.
```

---

### Part 4: Final Testing

**Testing Instructions:**

Run the application with all three tools enabled. You should observe:
- Time queries return accurate current time via get_current_time tool
- Math calculations are precise via Calculator tool
- String reversal is correct via reverse_string tool
- The AI seamlessly chooses and uses the appropriate tool for each query
- Verbose output shows the agent's reasoning and tool selection

---

## Expected Output

When running with all tools enabled and verbose mode, you should see output similar to:

```
🤖 JavaScript LangChain Agent Starting...

Running example queries:


📝 Query: What time is it right now?
──────────────────────────────────────────────────

[Agent reasoning and tool calls displayed in verbose mode]

✅ Result: The current date and time is Sun Jan 12 2026 14:30:45 GMT-0500 (Eastern Standard Time)


📝 Query: What is 25 * 4 + 10?
──────────────────────────────────────────────────

[Agent reasoning and tool calls displayed in verbose mode]

✅ Result: 110


📝 Query: Reverse the string 'Hello World'
──────────────────────────────────────────────────

[Agent reasoning and tool calls displayed in verbose mode]

✅ Result: dlroW olleH


🎉 Agent demo complete!
```

---

## Final Assessment

Test your understanding by completing this assessment:

### Weather Tool with Multi-Function Calling

**Objective:** Create a Weather Tool that demonstrates the AI's ability to chain multiple function calls together.

**Requirements:**

1. **Create a Weather Tool** using DynamicTool that:
   - Has name: "get_weather"
   - Has a description that explains it accepts a date parameter (formatted as "yyyy-MM-dd")
   - Returns "Sunny, 72°F" if the date matches today's date
   - Returns "Rainy, 55°F" for all other dates
   - Include proper description for the function

2. **Register the tool** in your tools array along with the existing get_current_time tool

3. **Test with a query** that requires two function calls:
   - Ask the AI: "What's the weather like today?"
   - The AI should:
     - First call get_current_time() to get today's date
     - Then call get_weather with that date to get the weather
     - Return a complete answer combining both pieces of information

**Success Criteria:**
- The AI successfully chains two function calls without explicit instruction
- Mock weather data is returned based on the current date
- The response is coherent and answers the original question

---

## Discussion Questions

After completing the lab, consider:

1. What's the difference between responses with and without tools?
2. Why are tools important for AI agents?
3. How does LangChain orchestrate the tool calls?
4. What is the role of the agent executor in this architecture?
5. How does the "openai-functions" agent type work?

---

## Extension Challenges

If you finish early, try:

### Additional Tools
1. Add a FileTool that can read/write text files using Node.js fs module
2. Create a WebSearchTool that simulates web search results
3. Add conversation memory to maintain context across multiple interactions using BufferMemory

### Cross-Cutting Concerns (Use Copilot to Help!)

**Logging & Observability**
4. Add comprehensive logging throughout the application
   - Log all AI requests and responses
   - Log tool calls with parameters and results
   - Use a logging library like winston or pino
   - Include timestamps and log levels (info, debug, error)
   - Ask Copilot: "Add logging using winston to track AI interactions and tool calls"

**Performance Monitoring**
5. Add performance metrics and timing
   - Measure response time for each AI query
   - Track tool execution duration using console.time() and console.timeEnd()
   - Log slow queries (over a threshold)
   - Ask Copilot: "Add performance monitoring to track query response times"

**Error Handling & Resilience**
6. Implement robust error handling
   - Add retry logic with exponential backoff for API failures
   - Handle rate limiting scenarios gracefully
   - Provide detailed error messages to users
   - Ask Copilot: "Add retry logic with exponential backoff for AI API calls"

**Input Validation**
7. Add input validation and sanitization
   - Validate user queries before sending to AI
   - Sanitize inputs to prevent injection attacks
   - Set maximum query length limits
   - Ask Copilot: "Add input validation to sanitize and validate user queries"

**Streaming Responses**
8. Implement streaming for real-time responses
   - Use the streaming capability of LangChain
   - Display responses as they are generated token by token
   - Handle streaming errors gracefully
   - Ask Copilot: "Add streaming support to display AI responses in real-time"

**Handling Rate Limit Responses (HTTP 429)**
9. Handle rate limiting responses from the API
    - Detect and catch HTTP 429 (Too Many Requests) errors
    - Parse retry-after headers from the response
    - Implement automatic retry after the specified delay
    - Display user-friendly messages when rate limited
    - Ask Copilot: "Add handling for HTTP 429 rate limit responses with automatic retry"

---

## Troubleshooting

### NPM Installation Issues

If you encounter dependency issues, try:
```bash
npm clean-install
```

### Node Version Issues

Ensure you're using Node.js 18 or later. Check with:
```bash
node --version
```

Use nvm (Node Version Manager) to switch versions if needed:
```bash
nvm install 18
nvm use 18
```

### Running the Application

Run the application with:
```bash
node app.js
```

### Environment Variable Issues

If the application can't find your GITHUB_TOKEN:
1. Verify the `.env` file exists in the project root directory
2. Check that the file contains: `GITHUB_TOKEN=your_token_here`
3. Make sure there are no extra spaces or quotes around the token value
4. Restart your terminal/IDE after creating the `.env` file