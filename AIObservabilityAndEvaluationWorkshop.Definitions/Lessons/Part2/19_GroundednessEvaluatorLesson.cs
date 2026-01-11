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
    informationalScreenMessage: "This lesson demonstrates the Groundedness, Groundedness Pro, and Ungrounded Attributes Evaluators, which assess whether the AI's response is grounded in factual information and can be verified against source material. It helps detect hallucinations.",
    inputPromptTitle: "Answer the question in the form of a sentence",
    inputPromptMessage: "Who was elected the 45th president of the United States?")]
public class GroundednessEvaluatorLesson(IServiceProvider sp, IChatClient chatClient, ILogger<GroundednessEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        GroundednessEvaluator eval1 = new();
        GroundednessProEvaluator eval2 = new();
        UngroundedAttributesEvaluator eval3 = new();
        CompositeEvaluator compositeEvaluator = new(eval1, eval2, eval3);

        string truth = "The 45th president of the United States is Donald Trump";    
        GroundednessEvaluatorContext context1 = new(truth);
        GroundednessProEvaluatorContext context2 = new(truth);
        UngroundedAttributesEvaluatorContext context3 = new(truth);
        
        EvaluationResult evaluationResult = await compositeEvaluator.EvaluateAsync("Who was elected the 45th president of the United States?",
            message,
            chatConfiguration: GetSafetyChatConfiguration(sp, chatClient, logger),
            additionalContext: [context1, context2, context3]);

        return evaluationResult;
    }
}

