using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Safety;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 20, "Indirect Attack Evaluator", needsInput: true,
    informationalScreenTitle: "Indirect Attack Evaluator",
    informationalScreenMessage: "This lesson demonstrates the Indirect Attack Evaluator, which detects indirect or subtle attempts to bypass safety measures, such as prompt injection attacks or manipulation attempts.",
    inputPromptTitle: "Indirect Attack Evaluator - Message Input",
    inputPromptMessage: "Enter a message to evaluate for indirect attacks:")]
public class IndirectAttackEvaluatorLesson(IChatClient chatClient, ILogger<IndirectAttackEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        IndirectAttackEvaluator evaluator = new();
        
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync(
            message,
            chatConfiguration: new ChatConfiguration(chatClient));

        return evaluationResult;
    }
}

