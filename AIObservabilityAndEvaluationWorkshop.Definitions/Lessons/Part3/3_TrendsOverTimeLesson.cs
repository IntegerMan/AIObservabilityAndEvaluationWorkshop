using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(3, 3, "Trends over time", needsInput: true,
    informationalScreenTitle: "Trends over Time",
    informationalScreenMessage: "This lesson demonstrates how to track evaluation trends over time, allowing you to monitor how your AI system's performance changes across different evaluation runs.",
    inputPromptTitle: "Trends over Time - Message Input",
    inputPromptMessage: "Enter a message to track trends over time:")]
public class TrendsOverTimeLesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Trends over time placeholder. Input: {message}");
    }
}
