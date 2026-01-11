using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Safety;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 12, "Content Harm Evaluator", needsInput: true,
    informationalScreenTitle: "Content Harm Evaluator",
    informationalScreenMessage: "The Content Harm Evaluator is similar to the RTC evaluator in that it combines multiple evaluators into a single evaluator. This combines the HateAndUnfairnessEvaluator, SelfHarmEvaluator, ViolenceEvaluator, and SexualEvaluator into a single new evaluator.",
    inputPromptTitle: "Enter something potentially bad",
    inputPromptMessage: "Caution: if you are on Azure, your input may also trigger Azure's Content Safety filters, resulting in an error")]
public class ContentHarmvaluatorLesson(IServiceProvider sp, IChatClient chatClient, ILogger<HateAndUnfairnessEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        ContentHarmEvaluator evaluator = new();
        
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync(
            message,
            chatConfiguration: GetSafetyChatConfiguration(sp, chatClient, logger));

        return evaluationResult;
    }
}

