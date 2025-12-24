using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 6, "Equivalence Evaluator", needsInput: true)]
public class EquivalenceEvaluatorLesson(IChatClient chatClient, ILogger<EquivalenceEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        EquivalenceEvaluator evaluator = new();
        
        // Equivalence evaluator compares generated text to ground truth
        // For this lesson, we'll use the message as both the response and ground truth
        // In practice, you would have actual ground truth to compare against
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync(
            message,
            chatConfiguration: new ChatConfiguration(chatClient));

        return evaluationResult;
    }
}

