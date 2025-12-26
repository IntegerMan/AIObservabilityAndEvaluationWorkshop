using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(1, 6, "MEAI Conversation History", needsInput: true,
    informationalScreenTitle: "Conversation History Context",
    informationalScreenMessage: "This lesson demonstrates conversation history. The AI has already been introduced to 'Matt' in a previous exchange:\n\n**Previous Conversation:**\n- User: \"Hi, I'm Matt.\"\n- Assistant: \"Hello Matt! How can I help you today?\"\n\nYour message will be added to this conversation history.",
    inputPromptTitle: "MEAI Conversation History - Add Your Message",
    inputPromptMessage: "Enter your message to continue the conversation with Matt:")]
public class MEAIConversationHistoryLesson(IChatClient chatClient) : LessonBase
{
    protected override async Task<string> RunAsync(string message)
    {
        List<ChatMessage> chatMessages =
        [
            new ChatMessage(ChatRole.System, "You are a helpful AI assistant."),
            new ChatMessage(ChatRole.User, "Hi, I'm Matt."),
            new ChatMessage(ChatRole.Assistant, "Hello Matt! How can I help you today?"),
            new ChatMessage(ChatRole.User, message)
        ];

        ChatResponse response = await chatClient.GetResponseAsync(chatMessages);
        return response.ToString();
    }
}
