using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 28, "NLP Evaluators", needsInput: true)]
public class NLPEvaluatorsLesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult($"NLP Evaluators placeholder. Input: {message}");
    }
}

