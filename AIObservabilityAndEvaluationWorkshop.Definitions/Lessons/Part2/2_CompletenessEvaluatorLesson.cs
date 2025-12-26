using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 2, "Completeness Evaluator", needsInput: true,
    informationalScreenTitle: "Completeness Evaluator",
    informationalScreenMessage: "This lesson demonstrates the Completeness Evaluator, which assesses whether the AI's response fully addresses the user's question or request. It checks if all aspects of the query are covered.",
    inputPromptTitle: "Your answer (hint: it doesn't)",
    inputPromptMessage: "Answer the question regarding a fictitious hat: Does this hat allow me to move objects with my mind?")]
public class CompletenessEvaluatorLesson(IChatClient chatClient, ILogger<CompletenessEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        CompletenessEvaluator evaluator = new();
        
        CompletenessEvaluatorContext context = new("The answer should state several advantages of our hats, but should clearly deny their ability to provide telekentic or other psyonic powers.");
        
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync("Does this hat allow me to move objects with my mind?" , 
            message,
            chatConfiguration: new ChatConfiguration(chatClient),
            additionalContext: [context]);

        return evaluationResult;
    }
}
