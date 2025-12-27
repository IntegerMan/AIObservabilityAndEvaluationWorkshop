using AIObservabilityAndEvaluationWorkshop.ConsoleRunner;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AIObservabilityAndEvaluationWorkshop.Tests;

public class FluencyEvaluatorTest
{
    [Fact]
    public async Task ChatResponse_ShouldPassFluencyEvaluation()
    {
        // Arrange
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.AddServiceDefaults();
        builder.Services.AddConfiguredChatClient(builder.Configuration);

        IHost host = builder.Build();
        await host.StartAsync();

        IChatClient chatClient = host.Services.GetRequiredService<IChatClient>();
        FluencyEvaluator evaluator = new();

        // Act
        string sampleMessage = "Hello, how are you?";
        ChatMessage[] messages = [new ChatMessage(ChatRole.User, sampleMessage)];
        ChatResponse response = await chatClient.GetResponseAsync(sampleMessage);
        
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync(
            messages,
            response,
            chatConfiguration: new ChatConfiguration(chatClient));

        // Assert
        foreach (var metric in evaluationResult.Metrics)
        {
            bool failed = metric.Value.Interpretation?.Failed ?? false;
            Assert.False(failed, 
                $"Evaluation metric '{metric.Key}' failed. Reason: {metric.Value.Reason}. " +
                $"Interpretation: {metric.Value.Interpretation?.Reason ?? "No interpretation provided"}");
        }

        await host.StopAsync();
    }
}

