using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 30, "Adding Metrics and Diagnostics", needsInput: true,
    informationalScreenTitle: "Adding Metrics and Diagnostics",
    informationalScreenMessage: "This lesson demonstrates how to add custom metrics and diagnostic information to evaluators, providing additional insights into evaluation results and helping with debugging and analysis.",
    inputPromptTitle: "Adding Metrics and Diagnostics - Message Input",
    inputPromptMessage: "Enter a message to evaluate with metrics and diagnostics:")]
public class AddingMetricsAndDiagnosticsLesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Adding Metrics and Diagnostics placeholder. Input: {message}");
    }
}

