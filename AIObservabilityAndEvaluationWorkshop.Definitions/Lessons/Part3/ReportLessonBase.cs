using System.Reflection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

public abstract class ReportLessonBase(IChatClient chatClient) : LessonBase
{
    protected static async Task<IEnumerable<ScenarioRunResult>> GetLatestResultsAsync(ReportingConfiguration reportConfig, int count = 5)
    {
        List<ScenarioRunResult> results = [];
        await foreach (var name in reportConfig.ResultStore.GetLatestExecutionNamesAsync(count: count))
        {
            await foreach (var result in reportConfig.ResultStore.ReadResultsAsync(name))
            {
                results.Add(result);
            }
        }
        return results;
    }

    protected async Task GetResponseAndEvaluateAsync(string message, string scenarioName, ReportingConfiguration reportConfig)
    {
        ScenarioRun run = await reportConfig.CreateScenarioRunAsync(scenarioName: scenarioName,
            iterationName: GetDiskFriendlyDateString());

        ChatMessage[] messages =
        [
            new ChatMessage(ChatRole.System, "You are a helpful assistant providing information about products on a corporate website. Please be curteous and professional at all times."),
            new ChatMessage(ChatRole.User, message)
        ];

        ChatResponse response = await chatClient.GetResponseAsync(messages);

        await run.EvaluateAsync(messages, response);
        
        await run.DisposeAsync(); // ensure it gets flushed / fully written
    }
    
    protected string GetReportFileName()
    {
        LessonAttribute lessonAttribute = GetLessonAttribute();
        return $"{lessonAttribute.Part}_{lessonAttribute.Order}_Report.html";
    }

    protected ChatConfiguration GetChatConfiguration() => new(chatClient);
    
    private static string GetDiskFriendlyDateString() => DateTime.Now.ToString("yyyyMMdd_HHmmss");

}