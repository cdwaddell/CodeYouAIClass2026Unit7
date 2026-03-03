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

            // Load agent prompts from templates
            var researcherPrompt = await LoadPromptFromTemplate("templates/researcher.json");
            var writerPrompt = await LoadPromptFromTemplate("templates/writer.json");
            var editorPrompt = await LoadPromptFromTemplate("templates/editor.json");

            // Create kernels for each agent
            var researcherKernel = CreateKernel(githubToken);
            var writerKernel = CreateKernel(githubToken);
            var editorKernel = CreateKernel(githubToken);

            // Connect to Tavily MCP server and add tools
            Console.WriteLine("Connecting to Tavily MCP server...");
            var mcpClient = await ConnectToTavilyMcpAsync(tavilyApiKey);
            var mcpTools = await mcpClient.ListToolsAsync();
            
            Console.WriteLine($"✓ Connected to Tavily MCP server");
            Console.WriteLine($"  Available MCP tools: {string.Join(", ", mcpTools.Select(t => t.Name))}");
            Console.WriteLine();
            
            // Convert MCP tools (AIFunction) to Semantic Kernel functions
            var kernelFunctions = new List<KernelFunction>();
            
            researcherKernel.Plugins.AddFromFunctions("tavily", kernelFunctions);

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

            var writerAgent = new ChatCompletionAgent
            {
                Name = WriterName,
                Instructions = writerPrompt,
                Kernel = writerKernel
            };

            var editorAgent = new ChatCompletionAgent
            {
                Name = EditorName,
                Instructions = editorPrompt,
                Kernel = editorKernel
            };

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
        }

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
    }

    // Template model for JSON deserialization
    class AgentTemplate
    {
        public string? Template { get; set; }
    }

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

}
