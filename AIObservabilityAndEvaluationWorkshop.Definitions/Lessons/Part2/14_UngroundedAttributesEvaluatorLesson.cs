using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Safety;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 14, "Ungrounded Attributes Evaluator", needsInput: true,
    informationalScreenTitle: "Ungrounded Attributes Evaluator",
    informationalScreenMessage: "This lesson demonstrates the Ungrounded Attributes Evaluator, which detects when responses make claims about attributes or characteristics that are not supported by the source material or context.",
    inputPromptTitle: "Ungrounded Attributes Evaluator - Message Input",
    inputPromptMessage: "Enter a message to evaluate for ungrounded attributes:")]
public class UngroundedAttributesEvaluatorLesson(IChatClient chatClient, ILogger<UngroundedAttributesEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        UngroundedAttributesEvaluator evaluator = new();
        
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync(
            message,
            chatConfiguration: new ChatConfiguration(chatClient));

        return evaluationResult;
    }
}

