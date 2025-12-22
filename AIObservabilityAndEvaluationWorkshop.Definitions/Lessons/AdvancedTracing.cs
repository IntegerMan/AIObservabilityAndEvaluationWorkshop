using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
public class AdvancedTracing : LessonBase
{
    public override string DisplayName => "Advanced Tracing";

    protected override async Task<string> RunAsync(string message)
    {
        return $"Processed: {message}";
    }
}
