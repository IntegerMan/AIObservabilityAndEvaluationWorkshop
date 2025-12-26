using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(3, 8, "Custom pass / fail", needsInput: true,
    informationalScreenTitle: "Custom Pass / Fail",
    informationalScreenMessage: "This lesson demonstrates how to define custom pass/fail criteria for evaluations, allowing you to set specific thresholds and conditions that determine whether an evaluation passes or fails.",
    inputPromptTitle: "Custom Pass / Fail - Message Input",
    inputPromptMessage: "Enter a message to evaluate with custom pass/fail criteria:")]
public class CustomPassFailLesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Custom pass / fail placeholder. Input: {message}");
    }
}
