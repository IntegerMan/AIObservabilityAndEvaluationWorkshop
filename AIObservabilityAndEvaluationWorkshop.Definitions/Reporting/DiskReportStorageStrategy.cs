using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.AI.Evaluation.Reporting.Formats.Html;
using Microsoft.Extensions.AI.Evaluation.Reporting.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Reporting;

public class DiskReportStorageStrategy(IConfiguration configuration, ILogger<DiskReportStorageStrategy> logger) : IReportStorageStrategy
{
    public Task<ReportingConfiguration> CreateConfigurationAsync(IEnumerable<IEvaluator> evaluators)
    {
        string projectRoot = Directory.GetCurrentDirectory();
        string storagePath = configuration["EvaluationResultsPath"] ?? Path.Combine(projectRoot, "EvaluationResults");

        if (!Path.IsPathRooted(storagePath)) storagePath = Path.GetFullPath(storagePath, projectRoot);

        if (!Directory.Exists(storagePath))
        {
            logger.LogInformation("Creating storage directory: {StoragePath}", storagePath);
            Directory.CreateDirectory(storagePath);
        }

        DiskBasedResultStore store = new(storagePath);
        return Task.FromResult(new ReportingConfiguration(evaluators, store));
    }

    public async Task<string> WriteReportAsync(ReportingConfiguration reportingConfig, ScenarioRunResult runResult)
    {
        string projectRoot = Directory.GetCurrentDirectory();
        string reportPath = configuration["ReportsPath"] ?? Path.Combine(projectRoot, "Reports");

        if (!Path.IsPathRooted(reportPath)) reportPath = Path.GetFullPath(reportPath, projectRoot);

        if (!Directory.Exists(reportPath))
        {
            logger.LogInformation("Creating report directory: {ReportPath}", reportPath);
            Directory.CreateDirectory(reportPath);
        }

        HtmlReportWriter writer = new(reportPath);
        
        try 
        {
            logger.LogInformation("Writing HTML report to {ReportPath}", Path.GetFullPath(reportPath));
            await writer.WriteReportAsync([runResult]);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError(ex, "Failed to write HTML report to disk due to permission error.");
            throw;
        }

        string[] files = Directory.GetFiles(reportPath, "*.html", SearchOption.AllDirectories);
        string? htmlFile = files.FirstOrDefault(f => f.EndsWith("index.html")) ?? files.FirstOrDefault();
        
        return htmlFile != null ? Path.GetFullPath(htmlFile) : Path.Combine(Path.GetFullPath(reportPath), "index.html");
    }
}
