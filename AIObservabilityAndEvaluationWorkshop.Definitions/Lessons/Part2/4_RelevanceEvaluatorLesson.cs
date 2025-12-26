using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 4, "Relevance Evaluator", needsInput: true,
    informationalScreenTitle: "Relevance Evaluator",
    informationalScreenMessage: "This lesson demonstrates the Relevance Evaluator, which assesses how relevant the AI's response is to the user's query. It checks if the response addresses the actual question asked.",
    inputPromptTitle: "Your Answer",
    inputPromptMessage: "Respond to the question 'How many git branches must a nerd pull down before you can call them a nerd?':")]
public class RelevanceEvaluatorLesson(IChatClient chatClient, ILogger<RelevanceEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        RelevanceEvaluator evaluator = new();
        
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync("How many git branches must a nerd pull down before you can call them a nerd?",
            message,
            chatConfiguration: new ChatConfiguration(chatClient));

        return evaluationResult;
    }
}

