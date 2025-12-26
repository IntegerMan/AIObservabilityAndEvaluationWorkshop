using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(1, 9, "EXERCISE 1", needsInput: true,
    informationalScreenTitle: "Exercise 1",
    informationalScreenMessage: "This is an exercise lesson. Apply what you've learned in the previous lessons to complete this exercise.",
    inputPromptTitle: "Exercise 1 - Input",
    inputPromptMessage: "Enter your input for Exercise 1:")]
public class Exercise1Lesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Exercise 1 placeholder. Input: {message}");
    }
}
