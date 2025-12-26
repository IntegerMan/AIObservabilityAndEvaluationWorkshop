using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(3, 6, "Azure report storage", needsInput: true,
    informationalScreenTitle: "Azure Report Storage",
    informationalScreenMessage: "This lesson demonstrates how to store evaluation reports in Azure Blob Storage, enabling cloud-based report storage and sharing capabilities.",
    inputPromptTitle: "Azure Report Storage - Message Input",
    inputPromptMessage: "Enter a message to generate a report stored in Azure:")]
public class AzureReportStorageLesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Azure report storage placeholder. Input: {message}");
    }
}
