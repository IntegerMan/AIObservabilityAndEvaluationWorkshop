using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 25, "EXERCISE 2", needsInput: true,
    informationalScreenTitle: "Exercise 2",
    informationalScreenMessage: "This is an exercise lesson. Apply what you've learned about evaluators in the previous lessons to complete this exercise.",
    inputPromptTitle: "Exercise 2 - Input",
    inputPromptMessage: "Enter your input for Exercise 2:")]
public class Exercise2Lesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Exercise 2 placeholder. Input: {message}");
    }
}

