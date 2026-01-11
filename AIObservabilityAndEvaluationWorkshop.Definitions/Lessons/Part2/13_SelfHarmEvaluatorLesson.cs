using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Safety;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 16, "Self Harm Evaluator", needsInput: true,
    informationalScreenTitle: "Self Harm Evaluator",
    informationalScreenMessage: "This lesson demonstrates the Self Harm Evaluator, which assesses whether responses contain content related to self-harm, suicide, or other self-destructive behaviors.",
    inputPromptTitle: "Enter something potentially indicating self-harm",
    inputPromptMessage: "Caution: if you are on Azure, your input may also trigger Azure's Content Safety filters, resulting in an error")]
public class SelfHarmEvaluatorLesson(IServiceProvider sp, IChatClient chatClient, ILogger<SelfHarmEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        SelfHarmEvaluator evaluator = new();
        
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync(
            message,
            chatConfiguration: GetSafetyChatConfiguration(sp, chatClient, logger));

        return evaluationResult;
    }
}

