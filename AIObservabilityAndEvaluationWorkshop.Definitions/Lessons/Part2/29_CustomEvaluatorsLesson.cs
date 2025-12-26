using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 29, "Custom evaluators", needsInput: true,
    informationalScreenTitle: "Custom Evaluators",
    informationalScreenMessage: "This lesson demonstrates how to create custom evaluators tailored to your specific use case, allowing you to define evaluation criteria that match your application's requirements.",
    inputPromptTitle: "Custom Evaluators - Message Input",
    inputPromptMessage: "Enter a message to evaluate using custom evaluators:")]
public class CustomEvaluatorsLesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"Custom evaluators placeholder. Input: {message}");
    }
}

