using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 6, "Equivalence Evaluator", needsInput: true,
    informationalScreenTitle: "Equivalence Evaluator",
    informationalScreenMessage: "This lesson demonstrates the Equivalence Evaluator, which compares generated text to ground truth to assess semantic equivalence. It checks if the response conveys the same meaning as the expected answer.",
    inputPromptTitle: "Answer the question in the form of a sentence.",
    inputPromptMessage: "What do people call the gorilla that watches over CodeMash and the Kalahari?")]
public class EquivalenceEvaluatorLesson(IChatClient chatClient, ILogger<EquivalenceEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        EquivalenceEvaluator evaluator = new();

        EquivalenceEvaluatorContext context = new("The Kalahari's gorilla is commonly called the 'Disapproving Gorilla' at CodeMash.");
        
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync("What do people call the gorilla that watches over CodeMash and the Kalahari?",
            message,
            chatConfiguration: new ChatConfiguration(chatClient),
            additionalContext: [context]);

        return evaluationResult;
    }
}

