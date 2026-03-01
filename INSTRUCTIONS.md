# AI Agent Lab - General Instructions

Welcome to the AI Agent Lab! In this course, you'll learn how to build AI agents using different programming languages and frameworks.

## Getting Started

Follow these steps to set up your lab environment and complete the assignments:

---

## Step 1: Create Your Own Git Repository

1. **Sign in to GitHub** at https://github.com
2. **Create a new repository** for your lab work:
   - Click the "+" icon in the top right corner
   - Select "New repository"
   - Name it: `ai-agent-lab` (or your preferred name)
   - Add a description: "AI Agent lab assignments"
   - Choose "Public"
   - **Do NOT** initialize with README, .gitignore, or license
   - Click "Create repository"
3. **Copy the repository URL** (you'll need this in Step 3)

---

## Step 2: Clone the Class Repository

Clone this class repository to get the starter code and instructions:

```bash
git clone https://github.com/CodeYouOrg/CodeYouAIClass2026Unit3.git
cd CodeYouAIClass2026Unit3
```

---

## Step 3: Choose Your Language and Framework

This course offers multiple language options. Choose ONE of the following based on your preference or instructor requirements:

**Important:** The sample directories contain completed reference implementations. You will build your own version from scratch by following the step-by-step instructions. The samples are there so you can see what you're working toward.

### Option 1: Java with Semantic Kernel
- **Sample Directory:** `java-semantickernel/` (reference implementation)
- **Best for:** Students familiar with Java and Maven

### Option 2: JavaScript with LangChain
- **Sample Directory:** `javascript-langchain/` (reference implementation)
- **Best for:** Students familiar with Node.js and npm

### Option 3: Python with LangChain
- **Sample Directory:** `python-langchain/` (reference implementation)
- **Best for:** Students familiar with Python and pip

### Option 4: C# with Semantic Kernel
- **Sample Directory:** `dotnet-semantickernel/` (reference implementation)
- **Best for:** Students familiar with .NET and C#

### Option 5: Build Your Own in Another Language (Advanced)
- **No sample provided** - you're on your own!
- **Best for:** Students who want to explore other languages like Go, Rust, Ruby, TypeScript, Kotlin, etc.
- **Requirements:**
  - Use an AI framework/library that supports function calling (LangChain, Semantic Kernel, or similar)
  - Connect to GitHub Models (https://models.github.ai/inference) using the `openai/gpt-4o` model
  - Implement the same three tools: Calculator, Time, and String Reversal
  - Follow the same general structure as the provided samples
  - Use GitHub Copilot to help you set up the project and implement the agent
- **Note:** You'll need to research the appropriate AI libraries for your chosen language and adapt the concepts from the provided instructions

---

## Step 4: Set Up Your Working Directory

**DO NOT copy the sample code!** You will build your AI agent from scratch by following the step-by-step instructions.

The sample directories in the class repository are reference implementations that show you what your final code should look like. You can refer to them if you get stuck, but the goal is to build your own version using GitHub Copilot and the provided prompts.

### Create your project directory:

```bash
# Navigate to your repository
mkdir [path/to/create/your/ai-agent-lab]

# Create a directory for your new project
# Use the same name as your chosen language for consistency
mkdir [your-project-name]
cd [your-project-name]
```

**Example:**
- If you chose Python: `mkdir python-langchain`
- If you chose JavaScript: `mkdir javascript-langchain`
- If you chose Java: `mkdir java-semantickernel`
- If you chose C#: `mkdir dotnet-semantickernel`

---

## Step 5: Initialize Your Repository

```bash
# Make sure you're in your repository root directory
cd [path/where/you/created/your/ai-agent-lab]

# Initialize git (if not already done)
git init

# Create a README file
echo "# AI Agent Lab" > README.md

# Add all files
git add .

# Make your first commit
git commit -m "Initial commit: Set up repository"

# Add your remote repository
git remote add origin [your-repository-url]

# Push to GitHub
git push -u origin main
```

**Note:** Replace `[your-repository-url]` with the URL you copied in Step 1.

---

## NOTE: Commit and Push Your Work Regularly

As you complete each section of the lab:

```bash
# Stage your changes
git add .

# Commit with a descriptive message
git commit -m "Complete Part 2: Added Calculator tool"

# Push to GitHub
git push
```

---

## Step 6: Follow the Language-Specific Instructions

Open the `INSTRUCTIONS.md` file from the **class repository** (CodeYouAIClass) for your chosen language. These instructions contain step-by-step prompts that you'll use with GitHub Copilot to build your AI agent from scratch.

**Example:**
- If you chose Python, open `CodeYouAIClass/python-langchain/INSTRUCTIONS.md`
- If you chose Java, open `CodeYouAIClass/java-semantickernel/INSTRUCTIONS.md`

**How it works:**
1. Read each numbered prompt in the INSTRUCTIONS.md file
2. Copy the prompt and give it to GitHub Copilot (via Copilot Chat)
3. Review and apply the code that Copilot generates
4. Test your code at each checkpoint
5. If you get stuck, you can peek at the sample code in the class repository

The instructions will guide you through:
1. Installing prerequisites
2. Setting up your development environment
3. Building an AI agent step-by-step using GitHub Copilot
4. Testing your agent with various queries
5. Optional extension challenges

---

**Best Practices:**
- Commit after completing each major prompt or section
- Write clear, descriptive commit messages
- Push your work regularly to avoid losing progress

---

## Step 8: Final Submission

When you've completed the lab:

1. **Make a final commit and push:**
   ```bash
   git add .
   git commit -m "Final submission: Complete AI Agent lab"
   git push
   ```

2. **Verify your repository:**
   - Go to your GitHub repository in a web browser
   - Confirm all files are present
   - Check that your latest commit is visible

3. **Submit your repository URL:**
   - Copy your repository URL (e.g., `https://github.com/your-username/ai-agent-lab`)
   - Submit this URL to your instructor through the designated submission method

---

## Need Help?

- **GitHub Copilot Chat:** Use `Ctrl+Alt+I` / `Cmd+Alt+I` in VS Code to ask questions
- **Instructor Office Hours:** Check your course syllabus for availability
- **Class Discussion Board:** Post questions for peer and instructor support
- **Documentation:**
  - [LangChain Python](https://python.langchain.com/)
  - [LangChain JavaScript](https://js.langchain.com/)
  - [Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/)
  - [GitHub Models](https://github.com/marketplace/models)

---

## Grading Criteria

Your lab will typically be evaluated on:

1. **Completion:** Did you complete all required prompts and sections?
2. **Functionality:** Does your AI agent work correctly for all test queries?
3. **Code Quality:** Is your code well-organized and properly formatted?
4. **Git Usage:** Did you make regular commits with descriptive messages?
5. **Extension Challenges (Optional):** Bonus points for completed extensions

Check with your instructor for specific grading rubrics.

---

## Tips for Success

1. **Read all instructions carefully** before starting to code
2. **Test frequently** after each major change
3. **Use GitHub Copilot** to help you understand errors and debug issues
4. **Don't skip the "without tools" test points** - they demonstrate why AI agents need tools
5. **Ask questions early** if you get stuck
6. **Have fun!** Building AI agents is an exciting and practical skill

---

## Extension Challenge: Try Another Language! 🚀

After completing your first AI agent, challenge yourself by building the same project in a different language:

### Why Try Another Language?

- **Deepen Your Understanding:** See how different frameworks approach AI agents
- **Compare and Contrast:** Notice similarities and differences in LangChain vs Semantic Kernel
- **Expand Your Skills:** Build proficiency in multiple programming languages
- **Portfolio Building:** Showcase your versatility to potential employers

### How to Do It:

1. **Create a new directory** in your repository for the second language
   ```bash
   cd [path/where/you/created/your/ai-agent-lab]
   mkdir [second-language-directory]
   ```

2. **Follow the instructions** for your chosen second language from the class repository

3. **Use GitHub Copilot** to help you adapt to the new language and framework

4. **Compare your implementations:**
   - Which language felt more natural to you?
   - Which framework (LangChain vs Semantic Kernel) did you prefer?
   - What are the key differences in how tools/plugins are defined?
   - How does async/await differ between languages?

5. **Commit and push** your second implementation
   ```bash
   git add .
   git commit -m "Complete second language: [Language Name] implementation"
   git push
   ```

### Suggested Combinations:

- **Started with Python?** Try JavaScript to see how LangChain works in a different language
- **Started with JavaScript?** Try Java or C# to experience Semantic Kernel
- **Started with Java?** Try Python for a more concise syntax approach
- **Started with C#?** Try JavaScript to see the differences in async patterns

**Bonus:** Once you've completed two languages, you'll have a great understanding of how AI agent frameworks work across different ecosystems!

---

Good luck with your AI agent lab! 🚀
