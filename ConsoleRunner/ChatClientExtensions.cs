using System.ClientModel;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI;

namespace AIObservabilityAndEvaluationWorkshop.ConsoleRunner;

public static class ChatClientExtensions
{
    public static IServiceCollection AddConfiguredChatClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddChatClient(sp =>
        {
            IChatClient client;

            string aiProvider = configuration["AIProvider"] ?? "Ollama";
            string modelName = configuration["AIModel"] ?? "llama3.2";
            string? endpoint = configuration["AIEndpoint"];
            string? key = configuration["AIKey"];
            bool useIdentity = configuration.GetValue("AIUseIdentity", false);

            if (aiProvider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(endpoint))
                {
                    throw new InvalidOperationException("Azure OpenAI requires an endpoint.");
                }

                if (useIdentity)
                {
                    client = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential()).GetChatClient(modelName).AsIChatClient();
                }
                else if (!string.IsNullOrEmpty(key))
                {
                    client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(key)).GetChatClient(modelName).AsIChatClient();
                }
                else
                {
                    throw new InvalidOperationException("Azure OpenAI requires either Identity or an API Key.");
                }
            }
            else if (aiProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new InvalidOperationException("OpenAI requires an API Key.");
                }

                OpenAIClientOptions? options = null;
                if (!string.IsNullOrEmpty(endpoint))
                {
                    options = new OpenAIClientOptions { Endpoint = new Uri(endpoint) };
                }

                client = new OpenAIClient(new ApiKeyCredential(key), options).GetChatClient(modelName).AsIChatClient();
            }
            else
            {
                // Default to Ollama
                string ollamaEndpoint = configuration.GetConnectionString(modelName)
                                     ?? configuration.GetConnectionString("ollama")
                                     ?? endpoint
                                     ?? "http://localhost:11434";

                if (!Uri.TryCreate(ollamaEndpoint, UriKind.Absolute, out Uri? ollamaUri))
                {
                    ollamaUri = new Uri("http://localhost:11434");
                }

                client = new OllamaChatClient(ollamaUri, modelName);
            }

            return client;
        })
        .UseFunctionInvocation()
        .Use((inner, sp) =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            bool enableSensitiveData = configuration.GetValue("EnableSensitiveDataLogging", true);

            return new OpenTelemetryChatClient(inner, loggerFactory.CreateLogger("Microsoft.Extensions.AI"))
            {
                EnableSensitiveData = enableSensitiveData
            };
        });

        return services;
    }
}
