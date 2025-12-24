using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 24, "Composite evaluator", needsInput: true)]
public class CompositeEvaluatorLesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Composite evaluator placeholder. Input: {message}");
    }
}
