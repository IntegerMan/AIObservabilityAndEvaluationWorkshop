using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(1, 8, "MEAI with Tools", needsInput: true)]
public class MEAIWithToolsLesson(IChatClient chatClient) : LessonBase
{
    protected override async Task<string> RunAsync(string message)
    {
        ChatOptions options = new()
        {
            Tools = [AIFunctionFactory.Create(GetWeather), AIFunctionFactory.Create(GetTime)]
        };

        ChatResponse response = await chatClient.GetResponseAsync(message, options);
        return response.ToString();
    }

    [Description("Gets the weather for a given city")]
    private string GetWeather(string city) => $"The weather in {city} is sunny.";

    [Description("Gets the current time")]
    private string GetTime() => DateTime.Now.ToString("T");
}
