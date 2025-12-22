using AIObservabilityAndEvaluationWorkshop.Definitions;
using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson("Metrics and Observability", needsInput: true)]
public class MetricsAndObservability : LessonBase
{
    protected override async Task<string> RunAsync(string message)
    {
        return $"Processed: {message}";
    }
}
