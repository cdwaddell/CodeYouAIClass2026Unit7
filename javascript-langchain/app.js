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

}

main().catch(console.error);
