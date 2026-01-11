using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Safety;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 15, "Hate And Unfairness Evaluator", needsInput: true,
    informationalScreenTitle: "Hate And Unfairness Evaluator",
    informationalScreenMessage: "This lesson demonstrates the Hate And Unfairness Evaluator, which assesses whether responses contain hate speech, discriminatory content, or unfair treatment of individuals or groups.",
    inputPromptTitle: "Enter something potentially hateful or unfair",
    inputPromptMessage: "Caution: if you are on Azure, your input may also trigger Azure's Content Safety filters, resulting in an error")]
public class HateAndUnfairnessEvaluatorLesson(IServiceProvider sp, IChatClient chatClient, ILogger<HateAndUnfairnessEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        HateAndUnfairnessEvaluator evaluator = new();
        
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync(
            message,
            chatConfiguration: GetSafetyChatConfiguration(sp, chatClient, logger));

        return evaluationResult;
    }
}

