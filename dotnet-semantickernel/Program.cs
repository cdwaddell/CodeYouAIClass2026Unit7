using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Configuration;

namespace SemanticKernelAgent
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🤖 C# Semantic Kernel Agent Starting...\n");

            // Load configuration from environment variables
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>()
                .Build();

            var githubToken = configuration["GITHUB_TOKEN"];
            
            if (string.IsNullOrEmpty(githubToken))
            {
                Console.WriteLine("❌ Error: GITHUB_TOKEN not found in environment variables.");
                Console.WriteLine("Set it using: $env:GITHUB_TOKEN=\"your-github-token-here\"");
                Console.WriteLine("Or use user secrets: dotnet user-secrets set \"GITHUB_TOKEN\" \"your-github-token-here\"");
                return;
            }

            // Create kernel with GitHub Models chat completion
            var builder = Kernel.CreateBuilder();

            builder.AddOpenAIChatCompletion(
                modelId: "openai/gpt-4o",
                apiKey: githubToken,
                endpoint: new Uri("https://models.github.ai/inference"));
            
            // Add plugins (tools) to the kernel
            builder.Plugins.AddFromType<TimePlugin>();
            builder.Plugins.AddFromType<MathPlugin>();
            builder.Plugins.AddFromType<StringPlugin>();
            
            var kernel = builder.Build();

            // Get chat completion service
            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            // Create chat history with system prompt
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("You are a professional and helpful AI assistant. Provide succinct, accurate responses.");
            
            // Example queries
            var queries = new[]
            {
                "What time is it right now?",
                "What is 25 * 4 + 10?",
                "Reverse the string 'Hello World'"
            };

            Console.WriteLine("Running example queries:\n");

            foreach (var query in queries)
            {
                Console.WriteLine($"\n📝 Query: {query}");
                Console.WriteLine(new string('─', 50));
                
                chatHistory.AddUserMessage(query);

                try
                {
                    // Enable auto function calling
                    var executionSettings = new OpenAIPromptExecutionSettings
                    {
                        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                    };

                    var result = await chatService.GetChatMessageContentAsync(
                        chatHistory,
                        executionSettings,
                        kernel);

                    Console.WriteLine($"\n✅ Result: {result.Content}\n");
                    chatHistory.AddAssistantMessage(result.Content ?? string.Empty);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error: {ex.Message}\n");
                }
            }

            Console.WriteLine("\n🎉 Agent demo complete!");
        }
    }
}
