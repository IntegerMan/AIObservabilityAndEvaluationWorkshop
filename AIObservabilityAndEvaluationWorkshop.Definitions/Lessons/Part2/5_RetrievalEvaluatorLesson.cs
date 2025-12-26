using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 5, "Retrieval Evaluator", needsInput: true,
    informationalScreenTitle: "Retrieval Evaluator",
    informationalScreenMessage: "This lesson demonstrates the Retrieval Evaluator, which assesses how well the AI retrieves and uses relevant information from context or knowledge bases to answer the query.",
    inputPromptTitle: "When and where does CodeMash take place?",
    inputPromptMessage: "Enter a message to evaluate for retrieval quality:")]
public class RetrievalEvaluatorLesson(IChatClient chatClient, ILogger<RetrievalEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        RetrievalEvaluator evaluator = new();

        // Note that we're not actually searching for this info. This evaluator just tests that our model is taking the input into account in its answer.
        string retrievedData = "CodeMash takes place every January in the Kalahari Convention Center in Sandusky, Ohio. In 2026 CodeMash takes place from January 13th to the 16th.";
        RetrievalEvaluatorContext context = new(retrievedData);
        
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync( "When and where does CodeMash take place?",
            message,
            chatConfiguration: new ChatConfiguration(chatClient),
            additionalContext: [context]);

        return evaluationResult;
    }
}

