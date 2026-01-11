using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 1, "Fluency Evaluator", needsInput: true,
    informationalScreenTitle: "Fluency Evaluator",
    informationalScreenMessage: "This lesson demonstrates the Fluency Evaluator, which assesses how natural and fluent the AI's response sounds. The evaluator checks for grammatical correctness, natural language flow, and overall readability.",
    inputPromptTitle: "Fluency Evaluator - Message Input",
    inputPromptMessage: "Enter a message to evaluate for fluency:")]
public class FluencyEvaluatorLesson(IChatClient chatClient, ILogger<FluencyEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        FluencyEvaluator evaluator = new();
        
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync(
            message,
            chatConfiguration: new ChatConfiguration(chatClient));

        return evaluationResult;
    }
}
