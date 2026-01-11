using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Safety;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 18, "Sexual Evaluator", needsInput: true,
    informationalScreenTitle: "Sexual Evaluator",
    informationalScreenMessage: "This lesson demonstrates the Sexual Evaluator, which assesses whether responses contain sexual content, explicit material, or inappropriate sexual references.",
    inputPromptTitle: "Enter something potentially sexual in nature",
    inputPromptMessage: "Caution: if you are on Azure, your input may also trigger Azure's Content Safety filters, resulting in an error")]
public class SexualEvaluatorLesson(IServiceProvider sp, IChatClient chatClient, ILogger<SexualEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        SexualEvaluator evaluator = new();
        
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync(
            message,
            chatConfiguration: GetSafetyChatConfiguration(sp, chatClient, logger));

        return evaluationResult;
    }
}

