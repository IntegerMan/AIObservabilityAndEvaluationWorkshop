using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(1, 8, "MEAI with Tools", needsInput: true,
    informationalScreenTitle: "MEAI with Tools",
    informationalScreenMessage: "This lesson demonstrates AI function calling (tools). The AI has access to two tools:\n\n1. **GetWeather(city)** - Gets the weather for a given city\n2. **GetTime()** - Gets the current time\n\nTry asking questions that would require these tools, such as:\n- \"What's the weather in Chicago?\"\n- \"What time is it?\"\n- \"What's the weather in New York and what time is it?\"",
    inputPromptTitle: "MEAI with Tools - Message Input",
    inputPromptMessage: "Enter a message that might require using tools (weather or time):")]
public class MEAIWithToolsLesson(IChatClient chatClient) : LessonBase
{
    protected override async Task<string> RunAsync(string message)
    {
        ChatOptions options = new()
        {
            Tools = [AIFunctionFactory.Create(GetWeather), AIFunctionFactory.Create(GetTime)],
            ToolMode = ChatToolMode.Auto
        };

        ChatResponse response = await chatClient.GetResponseAsync(message, options);
        return response.ToString();
    }

    [Description("Gets the weather for a given city")]
    private string GetWeather(string city) => $"The weather in {city} is sunny.";

    [Description("Gets the current time")]
    private string GetTime() => DateTime.Now.ToString("T");
}
