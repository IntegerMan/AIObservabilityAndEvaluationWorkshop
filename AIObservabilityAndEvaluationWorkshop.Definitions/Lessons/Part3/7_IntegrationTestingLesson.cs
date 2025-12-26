using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(3, 7, "Integration Testing", needsInput: true,
    informationalScreenTitle: "Integration Testing",
    informationalScreenMessage: "This lesson demonstrates how to use evaluators in integration tests, allowing you to automatically verify AI system quality as part of your testing pipeline.",
    inputPromptTitle: "Integration Testing - Message Input",
    inputPromptMessage: "Enter a message to test in an integration test scenario:")]
public class IntegrationTestingLesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Integration Testing placeholder. Input: {message}");
    }
}
