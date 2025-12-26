using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.NLP;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 21, "BLEU Evaluator", needsInput: true,
    informationalScreenTitle: "BLEU Evaluator",
    informationalScreenMessage: "This lesson demonstrates the BLEU (Bilingual Evaluation Understudy) Evaluator, which measures the similarity between generated text and reference text using n-gram precision. It's commonly used for machine translation evaluation.",
    informationalScreenSupportsMarkdown: false,
    inputPromptTitle: "Please translate this from English to French",
    inputPromptMessage: "Message to translate: 'Hello, I am a computer.'")]
public class BleuEvaluatorLesson(IChatClient chatClient, ILogger<BleuEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        BLEUEvaluator evaluator = new();
        BLEUEvaluatorContext context = new("Bonjour, je suis un ordinateur!");
        
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync(
            [
                new ChatMessage(ChatRole.System, "You are a translation bot translating inputs from English to French"),
                new ChatMessage(ChatRole.User, "Hello, I am a computer.")
            ],
            new ChatResponse(new ChatMessage(ChatRole.Assistant, message)),
            additionalContext: [context]);

        return evaluationResult;
    }
}

