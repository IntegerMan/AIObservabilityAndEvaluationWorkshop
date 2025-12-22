using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(1, 4, "Hello MEAI", needsInput: true)]
public class HelloMEAILesson(IChatClient chatClient) : LessonBase
{
    protected override async Task<string> RunAsync(string message)
    {
        ChatResponse response = await chatClient.GetResponseAsync(message);
        return response.ToString();
    }
}
