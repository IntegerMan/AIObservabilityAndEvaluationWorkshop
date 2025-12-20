// The console app receives the user input as a command-line argument from the AppHost.
// The AppHost uses IInteractionService to prompt the user, then passes the result here.
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using AIObservabilityAndEvaluationWorkshop.Definitions;

var builder = Host.CreateApplicationBuilder(args);

// Add Aspire service defaults for telemetry
builder.AddServiceDefaults();

var activitySource = new ActivitySource(builder.Environment.ApplicationName);

var host = builder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("ConsoleRunner started");

// Define output file path
var outputFilePath = Path.Combine(AppContext.BaseDirectory, "console_output.json");

// Create the root command
var rootCommand = new RootCommand("Console application for displaying messages");

// Add the display command
var displayCommand = new Command("display", "Display a message");
var messageArgument = new Argument<string>("message", "The message to display");
displayCommand.AddArgument(messageArgument);

displayCommand.SetHandler(async (string message) =>
{
    var commandLogger = host.Services.GetRequiredService<ILogger<Program>>();

    using var activity = activitySource.StartActivity("DisplayMessage");
    activity?.SetTag("message", message);

    commandLogger.LogInformation("Displaying message: {Message}", message);

    Console.WriteLine(message);

    // Create result object
    var result = new ConsoleResult
    {
        Success = true,
        Input = message,
        Output = message,
        ErrorMessage = null
    };

    // Write to output file
    try
    {
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(outputFilePath, json);
    }
    catch (Exception ex)
    {
        commandLogger.LogError(ex, "Failed to write to output file");

        // Write error result instead
        var errorResult = new ConsoleResult
        {
            Success = false,
            Input = message,
            Output = null,
            ErrorMessage = ex.Message
        };

        try
        {
            var errorJson = JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(outputFilePath, errorJson);
        }
        catch (Exception innerEx)
        {
            commandLogger.LogError(innerEx, "Failed to write error result to output file");
        }
    }

    return;
}, messageArgument);

rootCommand.AddCommand(displayCommand);

await host.StartAsync();

return await rootCommand.InvokeAsync(args);
