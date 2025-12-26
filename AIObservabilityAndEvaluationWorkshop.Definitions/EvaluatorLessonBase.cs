using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Safety;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions;

public abstract class EvaluatorLessonBase(ILogger<EvaluatorLessonBase> logger) : LessonBase
{
    private readonly ActivitySource _activitySource = new(typeof(EvaluatorLessonBase).FullName!);

    protected abstract Task<EvaluationResult> EvaluateAsync(string message);

    protected override async Task<string> RunAsync(string message)
    {
        EvaluationResult result = await EvaluateAsync(message);
        try
        {
            logger.LogInformation("Evaluated result: {result}", JsonSerializer.Serialize(result));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not serialize evaluation result: {message}", ex.Message);
        }

        using Activity? activity = _activitySource.StartActivity("Summarize EvaluationResult", ActivityKind.Internal);
        
        StringBuilder sb = new();
        int index = 0;
        foreach (var metric in result.Metrics)
        {
            index++;
            sb.AppendLine($"#### {metric.Key}");
            activity?.AddTag($"metric-{index}.Key", metric.Key);
            sb.AppendLine();
            sb.AppendLine(metric.Value.Reason);
            activity?.AddTag($"metric-{index}.Reason", metric.Value.Reason);
            sb.AppendLine();
            sb.AppendLine($"Rating: {metric.Value.Interpretation?.Rating} ({(int)(metric.Value.Interpretation?.Rating - 1 ?? 0)} / 5)");
            activity?.AddTag($"metric-{index}.Rating", metric.Value.Interpretation?.Rating);
            sb.AppendLine();
            sb.AppendLine($"Interpretation: {metric.Value.Interpretation?.Reason}" ?? "No reasoning");
            activity?.AddTag($"metric-{index}.Interpretation", metric.Value.Interpretation?.Reason);
            sb.AppendLine($"Result: {((metric.Value.Interpretation?.Failed ?? false) ? "FAIL" : "PASS")}");
            activity?.AddTag($"metric-{index}.Fail", metric.Value.Interpretation?.Failed);

            // Show any diagnostic information provided
            if (metric.Value.Diagnostics?.Count > 0)
            {
                sb.AppendLine("##### Diagnostics");
                sb.AppendLine();
                foreach (var diagnostic in metric.Value.Diagnostics)
                {
                    sb.AppendLine($"- {diagnostic.Message} ({diagnostic.Severity})");
                    activity?.AddEvent(new ActivityEvent($"metric-{index}.diagnostic: " + diagnostic.Message));

                    if (diagnostic.Severity == EvaluationDiagnosticSeverity.Error)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, diagnostic.Message);
                    }
                }

                sb.AppendLine();
            }

            // Show any context associated with the metric
            if (metric.Value.Context?.Count > 0)
            {
                sb.AppendLine("##### Context");
                sb.AppendLine();
                foreach (var contextKvp in metric.Value.Context)
                {
                    sb.AppendLine($"- {contextKvp.Key}: {contextKvp.Value.Name}");
                    // Note: there are also items associated with the context value if you want more details
                }

                sb.AppendLine();
            }

            // Show any metadata associated with the metric
            if (metric.Value.Metadata?.Count > 0)
            {
                sb.AppendLine("##### Metadata");
                sb.AppendLine();
                foreach (var metadataKvp in metric.Value.Metadata ?? new Dictionary<string, string>())
                {
                    sb.AppendLine($"- {metadataKvp.Key}: {metadataKvp.Value}");
                }

                sb.AppendLine();
            }
        }

        // Build the Markdown response
        return $"""
                ### Input
                {message}

                ### Evaluation Metrics
                {sb}
                """;
    }

    protected static ChatConfiguration GetSafetyChatConfiguration(IServiceProvider sp, IChatClient chatClient, ILogger<EvaluatorLessonBase> logger)
    {
        ContentSafetyServiceConfiguration? config = sp.GetService<ContentSafetyServiceConfiguration>();
        if (config == null)
        {
            logger.LogWarning("No content safety service configuration found. Likely not on Azure Identity or no AIFoundryProjectEndpoint configured. Expect Content Safety Evaluators to error");
            return new ChatConfiguration(chatClient);
        }

        logger.LogDebug("Content safety service configuration found");
        return config.ToChatConfiguration(chatClient);
    }
}