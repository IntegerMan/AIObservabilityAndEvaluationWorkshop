using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson("Advanced Tracing", needsInput: true)]
public class AdvancedTracing : LessonBase
{
    protected override async Task<string> RunAsync(string message)
    {
        return $"Processed: {message}";
    }
}
