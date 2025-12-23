using Azure.Identity;
using Azure.Storage.Files.DataLake;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.AI.Evaluation.Reporting.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Reporting;

public class AzureReportStorageStrategy(IConfiguration configuration, ILogger<AzureReportStorageStrategy> logger) : IReportStorageStrategy
{
    public Task<ReportingConfiguration> CreateConfigurationAsync(IEnumerable<IEvaluator> evaluators)
    {
        string? endpoint = configuration["AzureStorageAccountEndpoint"];
        string? container = configuration["AzureStorageContainer"];

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(container))
        {
            throw new InvalidOperationException("Azure storage configuration is missing. Please set AzureStorageAccountEndpoint and AzureStorageContainer.");
        }

        DataLakeDirectoryClient dataLakeDirectoryClient = new(
            new Uri(baseUri: new Uri(endpoint), relativeUri: container),
            credential: new DefaultAzureCredential());

        ReportingConfiguration reportingConfig = AzureStorageReportingConfiguration.Create(
            client: dataLakeDirectoryClient,
            evaluators: evaluators);

        return Task.FromResult(reportingConfig);
    }

    public Task<string> WriteReportAsync(ReportingConfiguration reportingConfig, ScenarioRunResult runResult)
    {
        // AzureStorageReportingConfiguration handles storage automatically when results are evaluated or stored.
        // However, we might still want to return a URL if possible, or a message.
        // For now, return a generic message as the specific URL might be complex to construct without more info.
        return Task.FromResult("Report stored in Azure Storage");
    }
}
