using AIObservabilityAndEvaluationWorkshop.Definitions;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson("Ollama Chat", needsInput: true)]
public class OllamaLesson(IChatClient chatClient) : LessonBase
{
    protected override async Task<string> RunAsync(string message)
    {
        ChatResponse response = await chatClient.GetResponseAsync(message);
        return response.ToString();
    }
}
