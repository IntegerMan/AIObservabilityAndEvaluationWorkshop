using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 3, "Coherence Evaluator", needsInput: true,
    informationalScreenTitle: "Coherence Evaluator",
    informationalScreenMessage: "This lesson demonstrates the Coherence Evaluator, which assesses how well-structured and logically consistent the AI's response is. It evaluates the flow of ideas and logical connections.",
    inputPromptTitle: "Enter a sentence or two:",
    inputPromptMessage: "Your input will be graded purely on its readability and user-friendliness.")]
public class CoherenceEvaluatorLesson(IChatClient chatClient, ILogger<CoherenceEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        CoherenceEvaluator evaluator = new();
        
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync(message,
            chatConfiguration: new ChatConfiguration(chatClient));

        return evaluationResult;
    }
}
