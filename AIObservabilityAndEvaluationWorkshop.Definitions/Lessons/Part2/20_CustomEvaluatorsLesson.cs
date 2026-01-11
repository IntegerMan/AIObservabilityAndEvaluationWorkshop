using AIObservabilityAndEvaluationWorkshop.Definitions.Evaluators;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 29, "Custom evaluators", needsInput: true,
    informationalScreenTitle: "Custom Evaluators",
    informationalScreenMessage: "This lesson demonstrates how to create custom evaluators tailored to your specific use case, allowing you to define evaluation criteria that match your application's requirements.",
    inputPromptTitle: "Custom Evaluators - Message Input",
    inputPromptMessage: "Enter a message to evaluate using custom evaluators:")]
public class CustomEvaluatorsLesson(ILogger<CustomEvaluatorsLesson> logger, IChatClient chatClient) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        BrevityEvaluator evaluator = new();

        EvaluationResult evaluationResult = await evaluator.EvaluateAsync(message, chatConfiguration: new ChatConfiguration(chatClient));

        return evaluationResult;
    }
}

