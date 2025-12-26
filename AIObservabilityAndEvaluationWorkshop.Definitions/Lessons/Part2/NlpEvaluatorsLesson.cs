using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.NLP;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 21, "NLP Evaluators", needsInput: true,
    informationalScreenTitle: "NLP Evaluators",
    informationalScreenMessage: "This lesson demonstrates the F1, BLEU (Bilingual Evaluation Understudy) and GLEU (Google's BLEU) Evaluators, which measure the similarity between generated text and reference text using n-gram precision. These are commonly used for machine translation evaluation.",
    informationalScreenSupportsMarkdown: false,
    inputPromptTitle: "Please translate this from English to French",
    inputPromptMessage: "Message to translate: 'Hello, I am a computer.'")]
public class NlpEvaluatorsLesson(ILogger<NlpEvaluatorsLesson> logger) : EvaluatorLessonBase(logger)
{
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        F1Evaluator f1Evaluator = new();
        BLEUEvaluator bleuEvaluator = new();
        GLEUEvaluator gleuEvaluator = new();
        
        CompositeEvaluator evaluator = new(f1Evaluator, bleuEvaluator, gleuEvaluator);
        
        string[] references = [
            "Bonjour, je suis un ordinateur!",  // Bing
            "Bonjour, je suis un ordinateur.",  // Google
            "Bonjour, je suis un ordinateur!",  // DeepL.com
            "Salut, je suis un ordinateur.",    // ChatGPT 5.2
            "Allô, je suis un ordinateur.",     // ChatGPT 5.2
            "Je suis un ordinateur. Bonjour."   // ChatGPT 5.2
        ];
        
        F1EvaluatorContext f1Context = new(references.First());
        BLEUEvaluatorContext bleuContext = new(references);
        GLEUEvaluatorContext gleuContext = new(references);
        
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync(
            [
                new ChatMessage(ChatRole.System, "You are a translation assistant translating inputs from English to French"),
                new ChatMessage(ChatRole.User, "Hello, I am a computer.")
            ],
            new ChatResponse(new ChatMessage(ChatRole.Assistant, message)),
            additionalContext: [bleuContext, gleuContext, f1Context]);

        return evaluationResult;
    }
}

