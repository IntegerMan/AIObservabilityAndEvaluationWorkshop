using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 25, "EXERCISE 3", needsInput: true,
    informationalScreenTitle: "Exercise 3",
    informationalScreenMessage: "This is an exercise lesson. Apply what you've learned about evaluators in the previous lessons to complete this exercise.",
    inputPromptTitle: "Exercise 3 - Input",
    inputPromptMessage: "Enter your input for Exercise 3:")]
public class Exercise3Lesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        /*
         * Exercise 3: Choose your own adventure! Either:
         *
         * - Create a custom evaluator that does one of a few specific things:
         *     - Fails messages that contain profanity
         *     - Scores messages based on how "enthusiastic" they are (presence of exclamation points, all-caps words, emojis, etc.)
         *     - Fails messages that have words longer than seven letters long in them
         *
         * Alternatively:
         * - Expand your previous evaluation to include content safety evaluators such as HateAndUnfairnessEvaluator and SelfHarmEvaluator. (Requires Microsoft Foundry and Identity auth)
         */

        return Task.FromResult($"Exercise 3 placeholder. Input: {message}");
    }
}

