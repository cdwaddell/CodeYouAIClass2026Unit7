package com.codeyou.agent;

import com.azure.ai.openai.OpenAIAsyncClient;
import com.azure.ai.openai.OpenAIClientBuilder;
import com.azure.core.credential.KeyCredential;
import com.microsoft.semantickernel.Kernel;
import com.microsoft.semantickernel.plugin.KernelPlugin;
import com.microsoft.semantickernel.plugin.KernelPluginFactory;
import com.microsoft.semantickernel.orchestration.InvocationContext;
import com.microsoft.semantickernel.orchestration.ToolCallBehavior;
import com.microsoft.semantickernel.services.chatcompletion.ChatCompletionService;
import com.microsoft.semantickernel.services.chatcompletion.ChatHistory;
import com.microsoft.semantickernel.services.chatcompletion.ChatMessageContent;
import io.github.cdimascio.dotenv.Dotenv;

public class App {
    public static void main(String[] args) {
        System.out.println("🤖 Java Semantic Kernel Agent Starting...\n");

        // Load environment variables from .env file
        Dotenv dotenv = Dotenv.configure()
                .ignoreIfMissing()
                .load();

        String githubToken = dotenv.get("GITHUB_TOKEN");
        if (githubToken == null || githubToken.isEmpty()) {
            githubToken = System.getenv("GITHUB_TOKEN");
        }

        if (githubToken == null || githubToken.isEmpty()) {
            System.out.println("❌ Error: GITHUB_TOKEN not found in environment variables.");
            System.out.println("Please create a .env file with your GitHub token:");
            System.out.println("GITHUB_TOKEN=your-github-token-here");
            return;
        }

        try {
            // Create OpenAI async client configured for GitHub Models
            OpenAIAsyncClient openAIAsyncClient = new OpenAIClientBuilder()
                    .endpoint("https://models.github.ai/inference")
                    .credential(new KeyCredential(githubToken))
                    .buildAsyncClient();

            // Create OpenAI chat completion service
            ChatCompletionService chatCompletionService = 
                com.microsoft.semantickernel.aiservices.openai.chatcompletion.OpenAIChatCompletion.builder()
                    .withOpenAIAsyncClient(openAIAsyncClient)
                    .withModelId("openai/gpt-4o")
                    .build();

            // Create kernel with plugins
            KernelPlugin timePlugin = KernelPluginFactory.createFromObject(new TimePlugin(), "TimePlugin");
            KernelPlugin mathPlugin = KernelPluginFactory.createFromObject(new MathPlugin(), "MathPlugin");
            KernelPlugin stringPlugin = KernelPluginFactory.createFromObject(new StringPlugin(), "StringPlugin");

            Kernel kernel = Kernel.builder()
                    .withAIService(ChatCompletionService.class, chatCompletionService)
                    .withPlugin(timePlugin)
                    .withPlugin(mathPlugin)
                    .withPlugin(stringPlugin)
                    .build();

            // Get chat completion service
            ChatCompletionService chatService = kernel.getService(ChatCompletionService.class);

            // Create chat history with system prompt
            ChatHistory chatHistory = new ChatHistory();
            chatHistory.addSystemMessage("You are a professional and helpful AI assistant. Provide succinct, accurate responses.");

            // Example queries
            String[] queries = {
                "What time is it right now?",
                "What is 25 * 4 + 10?",
                "Reverse the string 'Hello World'"
            };

            System.out.println("Running example queries:\n");

            for (String query : queries) {
                System.out.println("\n📝 Query: " + query);
                System.out.println("─".repeat(50));

                chatHistory.addUserMessage(query);

                try {
                    // Create invocation context with auto function calling
                    InvocationContext invocationContext = InvocationContext.builder()
                            .withToolCallBehavior(ToolCallBehavior.allowAllKernelFunctions(true))
                            .build();

                    ChatMessageContent<?> result = chatService.getChatMessageContentsAsync(
                            chatHistory,
                            kernel,
                            invocationContext
                    ).block().get(0);

                    System.out.println("\n✅ Result: " + result.getContent() + "\n");
                    chatHistory.addAssistantMessage(result.getContent());
                } catch (Exception e) {
                    System.out.println("❌ Error: " + e.getMessage() + "\n");
                }
            }

            System.out.println("\n🎉 Agent demo complete!");
        } catch (Exception e) {
            System.err.println("❌ Fatal error: " + e.getMessage());
            e.printStackTrace();
        }
    }
}
