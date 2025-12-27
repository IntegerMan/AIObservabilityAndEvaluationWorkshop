using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.AI.Evaluation.Reporting.Formats.Html;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(3, 1, "Report Generation", needsInput: true,
    informationalScreenTitle: "Report Generation",
    informationalScreenMessage:  "This lesson demonstrates how to generate evaluation reports. The system will evaluate your input using a Fluency Evaluator and generate an HTML report that you can view in your browser.",
    inputPromptTitle: "Customer Service Response Fluency",
    inputPromptMessage: "You're chatting with a corporate help bot running an online commerce applicaiton. Ask it a question and we'll evaluate the fluency of its response.")]
public class ReportGenerationLesson(
    IChatClient chatClient,
    IConfiguration configuration,
    ILogger<ReportGenerationLesson> logger,
    IEvaluationResultStore resultStore) : ReportLessonBase(chatClient)
{
    protected override async Task<string> RunAsync(string message)
    {
        IEvaluator evaluator = new FluencyEvaluator();
        ReportingConfiguration reportConfig = new([evaluator], 
            resultStore, 
            GetChatConfiguration(), 
            tags: ["CodeMash"]);

        await GetResponseAndEvaluateAsync(message, scenarioName: "Fluency Check for Customer Service Reply", reportConfig);

        IEnumerable<ScenarioRunResult> results = await GetLatestResultsAsync(reportConfig, count: 1);
        
        string filename = GetReportFileName();
        logger.LogDebug("Using report filename {Filename}", filename);
        
        // Get an asbolute path if this is a relative reference.
        string path = configuration["ReportsPath"] ?? "Reports";
        if (!Path.IsPathRooted(path))
        {
            path = Path.GetFullPath(path, Environment.CurrentDirectory);
        }
        path = Path.Combine(path, filename);
        
        logger.LogDebug("Using report location {Path}", path);

        HtmlReportWriter writer = new(path);
        await writer.WriteReportAsync(results);
        
        // Return the link in Markdown format
        Uri uri = new Uri(path);
        return $"""
                ### Evaluation and Report Generation Complete

                **Report location:**
                [{uri.AbsoluteUri}]({uri.AbsoluteUri})
                """;
    }
}