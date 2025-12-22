using AIObservabilityAndEvaluationWorkshop.Definitions;
using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
public class MetricsAndObservability : LessonBase
{
    public override string DisplayName => "Metrics and Observability";

    protected override async Task<string> RunAsync(string message)
    {
        return $"Processed: {message}";
    }
}
