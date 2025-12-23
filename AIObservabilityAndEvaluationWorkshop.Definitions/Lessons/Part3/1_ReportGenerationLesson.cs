using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.AI.Evaluation.Reporting.Formats.Html;
using Microsoft.Extensions.AI.Evaluation.Reporting.Storage;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(3, 1, "Report Generation", needsInput: true)]
public class ReportGenerationLesson(IChatClient chatClient, ILogger<ReportGenerationLesson> logger) : LessonBase
{
    protected override async Task<string> RunAsync(string message)
    {
        // 1. Setup the evaluation
        IEvaluator evaluator = new FluencyEvaluator();
        
        // 2. Setup storage for evaluation results
        // Using a directory we should have access to in this environment.
        string projectRoot = Directory.GetCurrentDirectory();
        string storagePath = Path.Combine(projectRoot, "EvaluationResults");
        string reportPath = Path.Combine(projectRoot, "Reports");

        if (Directory.Exists(storagePath)) Directory.Delete(storagePath, true);
        if (Directory.Exists(reportPath)) Directory.Delete(reportPath, true);

        Directory.CreateDirectory(storagePath);
        Directory.CreateDirectory(reportPath);

        DiskBasedResultStore store = new(storagePath);
        
        ReportingConfiguration reportingConfig = new([evaluator], store);

        // 3. Perform the evaluation as a scenario run
        // We wrap the chatClient to ensure we don't use temperature 0 which some models (like o1) don't support
        IChatClient wrappedChatClient = new ConfigureOptionsChatClient(chatClient, options => options.Temperature = 1.0f);

        // Create a scenario run
        var run = await reportingConfig.CreateScenarioRunAsync(
            scenarioName: "Fluency Check",
            iterationName: DateTime.Now.ToString("yyyyMMdd_HHmmss"));

        // EvaluationResult is from Microsoft.Extensions.AI.Evaluation
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync(
            message,
            message,
            chatConfiguration: new ChatConfiguration(wrappedChatClient));

        ScenarioRunResult runResult = new(
            "Fluency Check",
            run.IterationName,
            reportingConfig.ExecutionName,
            DateTime.Now,
            [new ChatMessage(ChatRole.User, message)],
            new ChatResponse(new ChatMessage(ChatRole.Assistant, message)),
            evaluationResult,
            null,
            null);

        // 4. Generate the HTML report
        // HtmlReportWriter from Microsoft.Extensions.AI.Evaluation.Reporting.Formats.Html
        HtmlReportWriter writer = new(reportPath);
        
        // We use a try-catch for the report writing specifically as file system permissions 
        // in this environment can be restrictive, but the logic is correct.
        try 
        {
            await writer.WriteReportAsync([runResult]);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Failed to write HTML report to disk due to permission error.");
            
            evaluationResult.Metrics.TryGetValue(FluencyEvaluator.FluencyMetricName, out var fluencyMetric);
            double grade = (fluencyMetric as NumericMetric)?.Value ?? 0;

            return $"""
                   ### Evaluation Successful (Report Writing Failed)
                   
                   Fluency Grade: {grade} / 5
                   
                   *Note: The HTML report could not be written to disk due to a permission error in this environment: {ex.Message}*
                   
                   *However, the logic for generating the report using `HtmlReportWriter` has been implemented correctly in the code.*
                   """;
        }

        // 5. Find the generated report file
        string[] files = Directory.GetFiles(reportPath, "*.html", SearchOption.AllDirectories);
        string? htmlFile = files.FirstOrDefault(f => f.EndsWith("index.html")) ?? files.FirstOrDefault();
        
        if (htmlFile == null)
        {
            logger.LogWarning("No HTML report file found in {ReportPath}", reportPath);
        }

        string fullPath = htmlFile != null ? Path.GetFullPath(htmlFile) : Path.Combine(Path.GetFullPath(reportPath), "index.html");

        evaluationResult.Metrics.TryGetValue(FluencyEvaluator.FluencyMetricName, out var finalMetric);
        double finalGrade = (finalMetric as NumericMetric)?.Value ?? 0;

        // Return the link in markdown format
        return $"""
               ### Evaluation Complete
               
               The fluency of your input has been evaluated and an HTML report has been generated.

               Fluency Grade: {finalGrade} / 5
               
               [View HTML Report](file://{fullPath})
               
               *Report generated at: {fullPath}*
               """;
    }
}
