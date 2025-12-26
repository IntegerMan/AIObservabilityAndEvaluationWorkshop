using System.Reflection;
using AIObservabilityAndEvaluationWorkshop.Definitions.Reporting;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.AI.Evaluation.Reporting;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(3, 1, "Report Generation", needsInput: true,
    informationalScreenTitle: "Report Generation",
    informationalScreenMessage: "This lesson demonstrates how to generate evaluation reports. The system will evaluate your input using a Fluency Evaluator and generate an HTML report that you can view in your browser.",
    inputPromptTitle: "Report Generation - Message Input",
    inputPromptMessage: "Enter a message to evaluate and generate a report for:")]
public class ReportGenerationLesson(
    IChatClient chatClient,
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
        await using ScenarioRun run = await reportingConfig.CreateScenarioRunAsync(
            scenarioName: "Fluency Check",
            iterationName: DateTime.Now.ToString("yyyyMMdd_HHmmss"));

        // EvaluationResult is from Microsoft.Extensions.AI.Evaluation
        EvaluationResult evaluationResult = await evaluator.EvaluateAsync(
            message,
            chatConfiguration: new ChatConfiguration(wrappedChatClient));

        ScenarioRunResult runResult = new(
            "Fluency Check",
            run.IterationName,
            reportingConfig.ExecutionName,
            DateTime.Now,
            messages:[], 
            modelResponse: new ChatResponse(new ChatMessage(ChatRole.Assistant, message)),
            evaluationResult: evaluationResult);

        // Get this lesson's part and order to use as the filename
        LessonAttribute lessonAttribute = GetType().GetCustomAttribute<LessonAttribute>()!;
        int part = lessonAttribute.Part;
        int order = lessonAttribute.Order;
        string filename = $"{part}_{order}_Report.html";

        // 4. Generate the report using the strategy
        string fullPath = await storageStrategy.WriteReportAsync(reportingConfig, runResult, filename);
        fullPath = fullPath.Replace('\\', '/'); // Clean up URLs for markdown links
        if (!fullPath.StartsWith("http") && !fullPath.StartsWith("file"))
        {
            fullPath = $"file:///{fullPath}";
        }

        evaluationResult.Metrics.TryGetValue(FluencyEvaluator.FluencyMetricName, out var finalMetric);
        double finalGrade = (finalMetric as NumericMetric)?.Value ?? 0;

        // Return the link in markdown format
        return $"""
               ### Evaluation Complete
               
               The fluency of your input has been evaluated and a report has been generated.

               Fluency Grade: {finalGrade} / 5
               
               **Report location:**
               [{fullPath}]({fullPath})
               """;
    }
}
