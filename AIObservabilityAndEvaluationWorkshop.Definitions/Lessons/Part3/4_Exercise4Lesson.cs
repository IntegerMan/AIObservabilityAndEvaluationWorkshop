using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(3, 4, "EXERCISE 4", needsInput: true,
    informationalScreenTitle: "Exercise 4",
    informationalScreenMessage: "This is an exercise lesson. Apply what you've learned about report generation, multiple scenarios, and trends in the previous lessons to complete this exercise.",
    inputPromptTitle: "Exercise 4 - Input",
    inputPromptMessage: "Enter your input for Exercise 4:")]
public class Exercise4Lesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Exercise 4 placeholder. Input: {message}");
    }
}
