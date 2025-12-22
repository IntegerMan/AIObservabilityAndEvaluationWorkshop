using AIObservabilityAndEvaluationWorkshop.Definitions;
using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
public class HelloWorkshop : LessonBase
{
    public override string DisplayName => "Hello Aspire and OpenTelemetry";

    public override bool NeedsInput => false;

    protected override Task<string> RunAsync(string message)
    {
        return Task.FromResult("""
                               # Hello World
                               We just executed some .NET code in the `HelloWorkshop.cs` file
                               (as well as other supporting files). 

                               We can now look at the various **Console** outputs, **Structured** logs, and OpenTelemetry **Traces** in the Aspire Dashboard - along with standard .NET **Metrics**.
                               """);
    }
}
