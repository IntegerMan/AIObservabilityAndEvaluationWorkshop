using AIObservabilityAndEvaluationWorkshop.ConsoleRunner;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AIObservabilityAndEvaluationWorkshop.Tests;

public class FluencyEvaluatorTest : ObservableTestBase
{
    [Fact]
    public async Task ChatResponse_ShouldPassFluencyEvaluation()
    {
        StartTest(nameof(ChatResponse_ShouldPassFluencyEvaluation));

        try
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
            bool allMetricsPassed = true;
            string? firstFailedMetric = null;

            foreach (var metric in evaluationResult.Metrics)
            {
                bool failed = metric.Value.Interpretation?.Failed ?? false;
                
                if (failed)
                {
                    allMetricsPassed = false;
                    firstFailedMetric ??= metric.Key;
                }

                Assert.False(failed, 
                    $"Evaluation metric '{metric.Key}' failed. Reason: {metric.Value.Reason}. " +
                    $"Interpretation: {metric.Value.Interpretation?.Reason ?? "No interpretation provided"}");
            }

            await host.StopAsync();
            CompleteTest(allMetricsPassed, 
                allMetricsPassed ? null : $"Metric '{firstFailedMetric}' failed evaluation");
        }
        catch (Exception ex)
        {
            RecordException(ex);
            throw;
        }
    }
}
