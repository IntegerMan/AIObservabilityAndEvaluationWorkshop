using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(3, 2, "Multiple Scenarios", needsInput: true,
    informationalScreenTitle: "Multiple Scenarios",
    informationalScreenMessage: "This lesson demonstrates how to run evaluations across multiple scenarios, allowing you to test your AI system with different inputs and compare results.",
    inputPromptTitle: "Multiple Scenarios - Message Input",
    inputPromptMessage: "Enter a message for the multiple scenarios evaluation:")]
public class MultipleScenariosLesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Multiple Scenarios placeholder. Input: {message}");
    }
}
