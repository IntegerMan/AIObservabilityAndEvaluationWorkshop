using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(1, 9, "EXERCISE 1", needsInput: true)]
public class Exercise1Lesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Exercise 1 placeholder. Input: {message}");
    }
}
