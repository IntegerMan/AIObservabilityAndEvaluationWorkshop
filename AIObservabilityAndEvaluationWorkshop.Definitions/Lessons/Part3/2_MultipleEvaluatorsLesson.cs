/*
using System.Reflection;
using AIObservabilityAndEvaluationWorkshop.Definitions.Reporting;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.AI.Evaluation.Reporting.Formats.Html;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(3, 2, "Multiple Scenarios", needsInput: true,
    informationalScreenTitle: "Multiple Scenarios",
    informationalScreenMessage: "This lesson uses multiple evaluators in the same scenario run, including evaluators requiring context.",
    inputPromptTitle: "What conference and room did this session take place?",
    inputPromptMessage: "Enter a sentence answering the sample prompt")]
public class MultipleEvaluatorsLesson(IReportStorageStrategy storageStrategy, IChatClient chatClient) : LessonBase
{
    protected override async Task<string> RunAsync(string message)
    {
        ReportingConfiguration reportingConfig = await storageStrategy.CreateConfigurationAsync([
            new FluencyEvaluator(),
            new CoherenceEvaluator(),
            new EquivalenceEvaluator()
        ], );

        await ExecuteEvaluationScenarioAsync(message, reportingConfig);

        // Enumerate the last 5 executions and add them to our list we'll use for reporting
        List<ScenarioRunResult> results = [];
        await foreach (var name in reportingConfig.ResultStore.GetLatestExecutionNamesAsync(count: 5))
        {
            await foreach (var result in reportingConfig.ResultStore.ReadResultsAsync(name))
            {
                results.Add(result);
            }
        }
        
        string filename = GetReportFilename();
        
        IEvaluationReportWriter writer = new HtmlReportWriter(filename);
        await writer.WriteReportAsync(results);

        // Return the link in markdown format
        return $"""
               ### Evaluation Complete
               
               Evaluation report completed.
               
               **Report location:**
               [{fullPath}]({fullPath})
               """;    }

    private string GetReportFilename()
    {
        LessonAttribute lessonAttribute = GetType().GetCustomAttribute<LessonAttribute>()!;
        int part = lessonAttribute.Part;
        int order = lessonAttribute.Order;
        string filename = $"{part}_{order}_Report.html";
        return filename;
    }

    private static async Task ExecuteEvaluationScenarioAsync(string message, ReportingConfiguration reportingConfig)
    {
        await using ScenarioRun run = await reportingConfig.CreateScenarioRunAsync(
            scenarioName: "Multiple Evaluators",
            iterationName: DateTime.Now.ToString("yyyyMMdd_HHmmss"));

        List<ChatMessage> messages = [
            new ChatMessage(ChatRole.System, "You are a helpful AI assistant that briefly answers questions using available facts."),
            new ChatMessage(ChatRole.User, "What conference and room did this session take place?")
        ];

        EquivalenceEvaluatorContext context = new("This session occurred in the Indigo Bay room at CodeMash 2026.");
        await run.EvaluateAsync(messages, new ChatResponse(new ChatMessage(ChatRole.Assistant, message)), additionalContext: [context]);
    }
}
*/