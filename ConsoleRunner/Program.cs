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

// Create the root command
var rootCommand = new RootCommand("Console application for displaying messages");

// Add the display command
var displayCommand = new Command("display", "Display a message");
var messageArgument = new Argument<string>("message", "The message to display");
displayCommand.AddArgument(messageArgument);

displayCommand.SetHandler(async (message) =>
{
    var commandLogger = host.Services.GetRequiredService<ILogger<Program>>();

    // Get output file path from environment variable
    var outputFile = Environment.GetEnvironmentVariable("CONSOLE_OUTPUT_FILE");
    Console.WriteLine($"Output file: {outputFile}");

    using var activity = activitySource.StartActivity("DisplayMessage");
    activity?.SetTag("message", message);

    commandLogger.LogInformation("Displaying message: {Message}", message);

    Console.WriteLine(message);

    // Only write to output file if environment variable is set
    if (!string.IsNullOrEmpty(outputFile))
    {
        commandLogger.LogInformation("Writing output to: {OutputFile}", outputFile);
        Console.WriteLine($"ConsoleRunner: Writing to file: {outputFile}");

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
            await File.WriteAllTextAsync(outputFile, json);
            Console.WriteLine($"ConsoleRunner: Successfully wrote {json.Length} characters to file");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ConsoleRunner: Failed to write to output file: {ex.Message}");
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
                await File.WriteAllTextAsync(outputFile, errorJson);
                Console.WriteLine($"ConsoleRunner: Wrote error result to file");
            }
            catch (Exception innerEx)
            {
                Console.WriteLine($"ConsoleRunner: Failed to write error result: {innerEx.Message}");
                commandLogger.LogError(innerEx, "Failed to write error result to output file");
            }
        }
    }
    else
    {
        Console.WriteLine("ConsoleRunner: CONSOLE_OUTPUT_FILE environment variable not set, skipping output file write");
        commandLogger.LogWarning("CONSOLE_OUTPUT_FILE environment variable not set, skipping output file write");
    }
}, messageArgument);

rootCommand.AddCommand(displayCommand);

await host.StartAsync();

return await rootCommand.InvokeAsync(args);
