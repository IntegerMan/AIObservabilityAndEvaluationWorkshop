using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 25, "EXERCISE 2", needsInput: true)]
public class Exercise2Lesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Exercise 2 placeholder. Input: {message}");
    }
}

