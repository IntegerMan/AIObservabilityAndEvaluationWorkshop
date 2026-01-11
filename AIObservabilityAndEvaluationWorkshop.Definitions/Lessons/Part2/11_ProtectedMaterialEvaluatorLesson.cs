using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Safety;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 13, "Protected Material Evaluator", needsInput: true,
    informationalScreenTitle: "Protected Material Evaluator",
    informationalScreenMessage: "This lesson demonstrates the Protected Material Evaluator, which assesses whether responses contain or reference protected material such as copyrighted content, proprietary information, or sensitive data.",
    inputPromptTitle: "Enter something potentially sensitive or copyrighted",
    inputPromptMessage: "Enter a message to evaluate for protected material:")]
public class ProtectedMaterialEvaluatorLesson(IServiceProvider sp, IChatClient chatClient, ILogger<ProtectedMaterialEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        ProtectedMaterialEvaluator evaluator = new();
        
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync(
            message,
            chatConfiguration: GetSafetyChatConfiguration(sp, chatClient, logger));

        return evaluationResult;
    }
}

