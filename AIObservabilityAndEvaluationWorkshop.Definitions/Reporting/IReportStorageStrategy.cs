using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Reporting;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Reporting;

public interface IReportStorageStrategy
{
    Task<ReportingConfiguration> CreateConfigurationAsync(IEnumerable<IEvaluator> evaluators);
    Task<string> WriteReportAsync(ReportingConfiguration reportingConfig, ScenarioRunResult runResult);
}
