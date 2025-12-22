using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(1, 5, "MEAI System Prompt", needsInput: true)]
public class MEAISystemPromptLesson(IChatClient chatClient) : LessonBase
{
    protected override async Task<string> RunAsync(string message)
    {
        List<ChatMessage> chatMessages =
        [
            new ChatMessage(ChatRole.System, "You are a helpful AI assistant that speaks like a pirate."),
            new ChatMessage(ChatRole.User, message)
        ];

        ChatResponse response = await chatClient.GetResponseAsync(chatMessages);
        return response.ToString();
    }
}
