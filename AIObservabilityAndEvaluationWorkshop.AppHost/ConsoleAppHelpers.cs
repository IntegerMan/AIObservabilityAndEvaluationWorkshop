using System.Text.RegularExpressions;

namespace AIObservabilityAndEvaluationWorkshop.AppHost;

public partial class ConsoleAppHelpers
{
    [GeneratedRegex(@"^CONSOLE_RESULT_FILE:\s*(?<filepath>.+)$", RegexOptions.Multiline)]
    public static partial Regex GetConsoleResultRegex();

}