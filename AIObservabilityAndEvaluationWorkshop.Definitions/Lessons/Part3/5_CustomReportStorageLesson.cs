using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(3, 5, "Custom report storage", needsInput: true,
    informationalScreenTitle: "Custom Report Storage",
    informationalScreenMessage: "This lesson demonstrates how to implement custom report storage strategies, allowing you to save evaluation reports to custom locations or systems beyond the default file system.",
    inputPromptTitle: "Custom Report Storage - Message Input",
    inputPromptMessage: "Enter a message to generate a report with custom storage:")]
public class CustomReportStorageLesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Custom report storage placeholder. Input: {message}");
    }
}
