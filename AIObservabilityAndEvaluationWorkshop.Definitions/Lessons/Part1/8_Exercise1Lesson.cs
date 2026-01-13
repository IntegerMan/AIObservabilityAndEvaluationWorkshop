using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(1, 8, "EXERCISE 1", needsInput: true,
    informationalScreenTitle: "Exercise 1",
    informationalScreenMessage:
    "This is an exercise lesson. Apply what you've learned in the previous lessons to build an agent that can analyze sentences.",
    inputPromptTitle: "Input Message",
    inputPromptMessage: "Enter a sentence for Exercise 1 to analyze:")]
public class Exercise1Lesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        /*
         * Your assignment is to create an AI prompt that can answer simple questions about the input sentence, things like:
         * - Was the sentiment of the sentence generally positive, negative, or neutral?
         * - Summarize the sentence in one short sentence.
         * - Why do you think the author wrote this sentence?
         * - Generate a Mermaid diagram representing the structure of the sentence.
         * - How many unique words are in the sentence?
         * - How many vowels are in the sentence?
         *
         * Tip: Provide a system prompt with clear instructions on how your system should behave.
         * Tip: You can use few-shot examples to guide the model on how to respond.
         * Tip: If your model is struggling on the words or vowels, consider adding a tool to help with the counting.
         * Tip: Finished early? Take a look at your system's behavior in OpenTelemetry, then try tweaking your prompt to give it a personality, or playing with the streaming methods to get back streaming responses.
         */

        // You'll probably need a chat client. You can get one from DependencyInjection via a constructor parameter.

        // You might want some messages or prior conversation history

        // ChatOptions are helpful if you plan on calling tools

        return Task.FromResult($"Exercise 1 placeholder. Input: {message}");
    }
}

