using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(3, 9, "EXERCISE 5", needsInput: true,
    informationalScreenTitle: "Exercise 5",
    informationalScreenMessage: "This is an exercise lesson. Apply what you've learned about report storage, integration testing, and custom pass/fail criteria in the previous lessons to complete this exercise.",
    inputPromptTitle: "Exercise 5 - Input",
    inputPromptMessage: "Enter your input for Exercise 5:")]
public class Exercise5Lesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Exercise 5 placeholder. Input: {message}");
    }
}
