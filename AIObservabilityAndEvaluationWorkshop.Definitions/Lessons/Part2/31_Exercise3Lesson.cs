using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 31, "EXERCISE 3", needsInput: true,
    informationalScreenTitle: "Exercise 3",
    informationalScreenMessage: "This is an exercise lesson. Apply what you've learned about evaluators, metrics, and diagnostics in the previous lessons to complete this exercise.",
    inputPromptTitle: "Exercise 3 - Input",
    inputPromptMessage: "Enter your input for Exercise 3:")]
public class Exercise3Lesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Exercise 3 placeholder. Input: {message}");
    }
}

