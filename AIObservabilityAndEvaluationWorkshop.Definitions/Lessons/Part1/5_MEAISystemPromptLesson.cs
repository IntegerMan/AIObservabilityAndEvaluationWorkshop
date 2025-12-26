using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(1, 5, "MEAI System Prompt", needsInput: true,
    informationalScreenTitle: "System Prompt Configuration",
    informationalScreenMessage: "This lesson demonstrates how to configure a system prompt for the AI model. The AI will respond as a helpful assistant that speaks like a pirate.",
    inputPromptTitle: "MEAI System Prompt - Message Input",
    inputPromptMessage: "Enter a message to send to the AI (it will respond as a pirate):")]
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
