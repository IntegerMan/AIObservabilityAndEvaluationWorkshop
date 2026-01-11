using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 25, "EXERCISE 2", needsInput: true,
    informationalScreenTitle: "Exercise 2",
    informationalScreenMessage: "This is an exercise lesson. Apply what you've learned about evaluators in the previous lessons to complete this exercise.",
    inputPromptMessage: "The user is asking 'What does your organization do?':",
    inputPromptTitle: "What does your bot say?")]
public class Exercise2Lesson(ILogger<Exercise2Lesson> logger, IChatClient chatClient) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        /*
         * You are tasked with evaluating the responses of an online chat bot embedded on your organization's home page.
         * Build a CompositeEvaluator that includes several different specific evaluators relevant to your bot's standard use case.
         * Try to include at least one evaluator that requires context. Note that many evaluators will also require you provide a conversation
         * history to them to be effective.
         */

        // Replace this with the results of your CompositeEvaluator's evaluation
        return await Task.FromResult(new EvaluationResult({});
    }
}

