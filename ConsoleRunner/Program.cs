using System.CommandLine;
using System.Diagnostics;
using AIObservabilityAndEvaluationWorkshop.ConsoleRunner;
using AIObservabilityAndEvaluationWorkshop.Definitions;
using AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddScoped<ExecuteLessonCommand>();

builder.Services.AddConfiguredChatClient(builder.Configuration);

builder.Services.Scan(scan => scan
    .FromAssemblyOf<HelloWorkshop>()
    .AddClasses(classes => classes.AssignableTo<LessonBase>())
    .As<LessonBase>()
    .WithScopedLifetime());

IHost host = builder.Build();

// Log OTEL environment variables on startup
TelemetryDiagnostics.LogOtelEnvironmentVariables(host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(TelemetryDiagnostics)));

await host.StartAsync();

// Add the execute-lesson command that allows users to send an input in through a message parameter
 Command executeLessonCommand = new("execute-lesson", "Execute a lesson with a message");
Argument<string> messageArgument = new ("message", "The message to pass to the lesson");
Argument<string> lessonDisplayNameArgument = new ("lesson-display-name", "The lesson display name");
executeLessonCommand.AddArgument(messageArgument);
executeLessonCommand.AddArgument(lessonDisplayNameArgument);

ExecuteLessonCommand command = host.Services.GetRequiredService<ExecuteLessonCommand>();
executeLessonCommand.SetHandler(command.ExecuteAsync, messageArgument, lessonDisplayNameArgument);

// Create the root command that routes inputs to other commands
RootCommand rootCommand = new("Console application for running lessons");
rootCommand.AddCommand(executeLessonCommand);

// Add default handler for when no command is specified
rootCommand.SetHandler(async () =>
{
    ILogger logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DefaultCommand");
    ActivitySource activitySource = new(typeof(Program).FullName!);

    // Create service instance with null input for default command
    AspireService aspireService = new(activitySource, logger, null);

    // Log the console error using the helper
    aspireService.LogError(
        "Please use the 'Start with Input' command to run this application",
        null,
        ("command.name", "default")
    );

    await Task.CompletedTask;
});

int result = await rootCommand.InvokeAsync(args);

// For console apps, give telemetry time to be exported before exiting
// This ensures all telemetry is exported to Aspire before the app terminates
await Task.Delay(TimeSpan.FromSeconds(2));

return result;
