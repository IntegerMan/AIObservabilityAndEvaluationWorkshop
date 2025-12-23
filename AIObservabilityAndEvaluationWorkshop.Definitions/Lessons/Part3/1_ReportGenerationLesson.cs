using AIObservabilityAndEvaluationWorkshop.Definitions.Reporting;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(3, 1, "Report Generation", needsInput: true)]
public class ReportGenerationLesson(
    IChatClient chatClient,
    ILogger<ReportGenerationLesson> logger,
    IConfiguration configuration,
    IReportStorageStrategy storageStrategy) : LessonBase
{
    protected override async Task<string> RunAsync(string message)
    {
        // 1. Setup the evaluation
        IEvaluator evaluator = new FluencyEvaluator();
        
        // 2. Setup storage for evaluation results
        ReportingConfiguration reportingConfig = await storageStrategy.CreateConfigurationAsync([evaluator]);

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

        // 4. Generate the report using the strategy
        string fullPath;
        try 
        {
            fullPath = await storageStrategy.WriteReportAsync(reportingConfig, runResult);
        }
        catch (UnauthorizedAccessException ex)
        {
            evaluationResult.Metrics.TryGetValue(FluencyEvaluator.FluencyMetricName, out var fluencyMetric);
            double grade = (fluencyMetric as NumericMetric)?.Value ?? 0;

            return $"""
                   ### Evaluation Successful (Report Writing Failed)
                   
                   Fluency Grade: {grade} / 5
                   
                   *Note: The HTML report could not be written due to a permission error: {ex.Message}*
                   """;
        }

        evaluationResult.Metrics.TryGetValue(FluencyEvaluator.FluencyMetricName, out var finalMetric);
        double finalGrade = (finalMetric as NumericMetric)?.Value ?? 0;

        string reportLink = fullPath.StartsWith("http") || fullPath.StartsWith("file") 
            ? $"[View Report]({fullPath})" 
            : (Path.IsPathRooted(fullPath) ? $"[View Report](file://{fullPath})" : fullPath);

        // Return the link in markdown format
        return $"""
               ### Evaluation Complete
               
               The fluency of your input has been evaluated and a report has been generated.

               Fluency Grade: {finalGrade} / 5
               
               {reportLink}
               
               *Report location: {fullPath}*
               """;
    }
}
