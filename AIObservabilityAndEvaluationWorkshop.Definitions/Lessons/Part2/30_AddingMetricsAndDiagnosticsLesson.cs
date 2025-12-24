using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 30, "Adding Metrics and Diagnostics", needsInput: true)]
public class AddingMetricsAndDiagnosticsLesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Adding Metrics and Diagnostics placeholder. Input: {message}");
    }
}

