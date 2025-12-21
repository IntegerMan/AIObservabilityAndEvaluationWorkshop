using System.Text.RegularExpressions;

namespace AIObservabilityAndEvaluationWorkshop.AppHost;

public partial class ConsoleAppHelpers
{
    [GeneratedRegex(@"^CONSOLE_RESULT:\s*(?<json>.*)$", RegexOptions.Multiline)]
    public static partial Regex GetConsoleResultRegex();

}