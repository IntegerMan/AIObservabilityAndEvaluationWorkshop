using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(1, 3, "Hello MEAI", needsInput: true,
    inputPromptTitle: "Hello MEAI - Message Input",
    inputPromptMessage: "Enter a message to send to the AI model:")]
public class HelloMEAILesson(IChatClient chatClient) : LessonBase
{
    protected override async Task<string> RunAsync(string message)
    {
        ChatResponse response = await chatClient.GetResponseAsync(message);
        return response.ToString();
    }
}
