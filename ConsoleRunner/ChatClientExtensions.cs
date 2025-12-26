using System.ClientModel;
using System.ClientModel.Primitives;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation.Safety;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI;

namespace AIObservabilityAndEvaluationWorkshop.ConsoleRunner;

public static class ChatClientExtensions
{
    public static IServiceCollection AddConfiguredChatClient(this IServiceCollection services,
        IConfiguration configuration)
    {
        string aiProvider = configuration["AIProvider"] ?? "Ollama";
        bool useIdentity = configuration.GetValue("AIUseIdentity", false);
        string? endpoint = configuration["AIEndpoint"];

        services.AddChatClient(_ =>
            {
                string modelName = configuration["AIModel"] ?? "llama3.2";
                string? key = configuration["AIKey"];
                bool allowUntrustedCertificates = configuration.GetValue("AllowUntrustedCertificates", false);

                return aiProvider.ToLowerInvariant() switch
                {
                    "azure" => ConfigureAzureClient(endpoint, allowUntrustedCertificates, useIdentity,
                        modelName, key),
                    "openai" => ConfigureOpenAiClient(key, endpoint, allowUntrustedCertificates, modelName),
                    _ => ConfigureOllamaClient(configuration, modelName, endpoint)
                };
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
        
        // We can only use Content Safety when on Azure Identity
        if (aiProvider.ToLowerInvariant() == "azure" && useIdentity && !string.IsNullOrEmpty(endpoint))
        {
            services.AddScoped<ContentSafetyServiceConfiguration>(_ => new(new DefaultAzureCredential(), new Uri(endpoint)));
        }

        return services;
    }

    private static IChatClient ConfigureOllamaClient(IConfiguration configuration, string modelName, string? endpoint)
    {
        string ollamaEndpoint = configuration.GetConnectionString(modelName)
                                ?? configuration.GetConnectionString("ollama")
                                ?? endpoint
                                ?? "http://localhost:11434";

        if (!Uri.TryCreate(ollamaEndpoint, UriKind.Absolute, out Uri? ollamaUri))
        {
            ollamaUri = new Uri("http://localhost:11434");
        }

        return new OllamaChatClient(ollamaUri, modelName);
    }

    private static IChatClient ConfigureOpenAiClient(string? key, string? endpoint, bool allowUntrustedCertificates,
        string modelName)
    {
        IChatClient client;
        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException("OpenAI requires an API Key.");
        }

        OpenAIClientOptions openaiOptions = new();
        if (!string.IsNullOrEmpty(endpoint))
        {
            openaiOptions.Endpoint = new Uri(endpoint);
        }

        if (allowUntrustedCertificates)
        {
            openaiOptions.Transport = new HttpClientPipelineTransport(new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            }));
        }

        client = new OpenAIClient(new ApiKeyCredential(key), openaiOptions).GetChatClient(modelName).AsIChatClient();
        return client;
    }

    private static IChatClient ConfigureAzureClient(string? endpoint, bool allowUntrustedCertificates, bool useIdentity,
        string modelName, string? key)
    {
        IChatClient client;
        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException("Azure OpenAI requires an endpoint.");
        }

        AzureOpenAIClientOptions azureOptions = new();
        if (allowUntrustedCertificates)
        {
            azureOptions.Transport = new HttpClientPipelineTransport(new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            }));
        }

        Uri uri = new Uri(endpoint);

        if (useIdentity)
        {
            // NOTE: This is not acceptable for production, but helps dodge issues with cached credentials. You'll need to use AZ LOGIN to provide this credential
            client = new AzureOpenAIClient(uri, new AzureCliCredential(), azureOptions).GetChatClient(modelName)
                .AsIChatClient();
        }
        else if (!string.IsNullOrEmpty(key))
        {
            client = new AzureOpenAIClient(uri, new ApiKeyCredential(key), azureOptions).GetChatClient(modelName)
                .AsIChatClient();
        }
        else
        {
            throw new InvalidOperationException("Azure OpenAI requires either Identity or an API Key.");
        }

        return client;
    }
}