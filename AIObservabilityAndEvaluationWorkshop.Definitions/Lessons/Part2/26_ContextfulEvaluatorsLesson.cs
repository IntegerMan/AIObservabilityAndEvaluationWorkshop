using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 26, "Contextful evaluators", needsInput: true)]
public class ContextfulEvaluatorsLesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Contextful evaluators placeholder. Input: {message}");
    }
}

