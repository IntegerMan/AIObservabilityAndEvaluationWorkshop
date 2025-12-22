using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(1, 10, "Setup for Cloud AI", needsInput: false)]
public class SetupForCloudAILesson : LessonBase
{
    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult("Setup for Cloud AI placeholder");
    }
}
