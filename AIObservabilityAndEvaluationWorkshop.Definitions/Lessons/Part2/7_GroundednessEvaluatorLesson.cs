using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.AI.Evaluation.Safety;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 7, "Groundedness Evaluator", needsInput: true,
    informationalScreenTitle: "Groundedness Evaluator",
    informationalScreenMessage: "This lesson demonstrates the Groundedness and Groundedness Pro Evaluators, which assess whether the AI's response is grounded in factual information and can be verified against source material. It helps detect hallucinations.",
    inputPromptTitle: "Answer the question in the form of a sentence",
    inputPromptMessage: "Who was elected the 47th president of the United States?")]
public class GroundednessEvaluatorLesson(IChatClient chatClient, ILogger<GroundednessEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        GroundednessEvaluator eval1 = new();
        GroundednessProEvaluator eval2 = new();
        CompositeEvaluator compositeEvaluator = new(eval1, eval2);

        string truth = "The 47th president of the United States is Donald Trump";    
        GroundednessEvaluatorContext context1 = new(truth);
        GroundednessProEvaluatorContext context2 = new(truth);
        
        EvaluationResult evaluationResult = await compositeEvaluator.EvaluateAsync("Who was elected the 47th president of the United States?",
            message,
            chatConfiguration: new ChatConfiguration(chatClient),
            additionalContext: [context1, context2]);

        return evaluationResult;
    }
}

