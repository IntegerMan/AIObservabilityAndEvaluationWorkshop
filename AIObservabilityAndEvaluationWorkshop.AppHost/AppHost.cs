#pragma warning disable ASPIREINTERACTION001 // Interaction Service is for evaluation purposes only

using Microsoft.Extensions.DependencyInjection;
using Projects;
using System.Text.Json;
using AIObservabilityAndEvaluationWorkshop.Definitions;

var builder = DistributedApplication.CreateBuilder(args);

// Create a temporary file for the console app output
var tempOutputFile = Path.GetTempFileName();
var tempOutputJsonFile = Path.ChangeExtension(tempOutputFile, ".json");

// Ensure the temporary JSON file doesn't exist initially
if (File.Exists(tempOutputJsonFile))
{
    File.Delete(tempOutputJsonFile);
}

string[] appArgs = [];

// Add the console app project (without WithExplicitStart so it doesn't auto-start)
IResourceBuilder<ProjectResource> consoleAppBuilder =
    builder.AddProject<AIObservabilityAndEvaluationWorkshop_ConsoleRunner>("console-app")
        .WithEnvironment("CONSOLE_OUTPUT_FILE", tempOutputJsonFile)
        //.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:19288")
        .WithExplicitStart()
        .WithArgs(context =>
        {
            context.Args.Clear();
            foreach (var arg in appArgs)
            {
                context.Args.Add(arg);
            }
        });

// Subscribe to console app resource ready event to capture and display output after completion
consoleAppBuilder.OnResourceReady(async (resource, readyEvent, cancellationToken) =>
{
    try
    {
        Console.WriteLine("AppHost: OnResourceReady event fired");

        var interactionSvc = readyEvent.Services.GetRequiredService<IInteractionService>();
        Console.WriteLine($"AppHost: Interaction service available in OnResourceReady: {interactionSvc.IsAvailable}");

        // Use the temporary file we created for output
        var outputFilePath = tempOutputJsonFile;
        Console.WriteLine($"AppHost: Watching for output file: {outputFilePath}");

        // Start watching for the output file
        await WatchForOutputFileAsync(outputFilePath, interactionSvc);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"AppHost: Exception in OnResourceReady handler: {ex.Message}");
        Console.WriteLine($"AppHost: Stack trace: {ex.StackTrace}");
    }
});

// Add a custom command that prompts for input and starts the resource
consoleAppBuilder.WithCommand("start-with-input", "Start with Input", async (context) =>
{
    var interactionService = context.ServiceProvider.GetRequiredService<IInteractionService>();

    if (!interactionService.IsAvailable)
    {
        return new ExecuteCommandResult { Success = false, ErrorMessage = "Interaction service not available" };
    }

    // Prompt the user for input
    InteractionResult<InteractionInput> result = await interactionService.PromptInputAsync(
        title: "User Input",
        message: "Please enter some text:",
        input: new InteractionInput
        {
            Name = "UserInput",
            InputType = InputType.Text,
            Required = true,
            Placeholder = "Enter your message here"
        });

    if (result.Canceled)
    {
        await interactionService.PromptNotificationAsync("Cancelled", "Operation cancelled by user.");
        return new ExecuteCommandResult { Success = false, ErrorMessage = "User cancelled input" };
    }

    appArgs = ["display", result.Data?.Value ?? "Hello, World!"];

    Console.WriteLine($"AppHost: Starting console app with args: {string.Join(", ", appArgs)}");

    // Simply start the resource
    var commandService = context.ServiceProvider.GetRequiredService<ResourceCommandService>();
    return await commandService.ExecuteCommandAsync(context.ResourceName, "resource-start");
},
new CommandOptions
{
    Description = "Configure the console app with user input and start it",
    IconName = "Play",
    IsHighlighted = true
});

builder.Build().Run();

async Task WatchForOutputFileAsync(string outputFilePath, IInteractionService interactionSvc)
{
    // Poll for the output file to be created and written
    var fileFound = false;
    var startTime = DateTime.UtcNow;
    var pollCount = 0;

    // Poll every 100ms for up to 30 seconds
    while (!fileFound && (DateTime.UtcNow - startTime).TotalSeconds < 30)
    {
        pollCount++;
        if (File.Exists(outputFilePath))
        {
            try
            {
                // Check if file has content (not empty or just created)
                var fileInfo = new FileInfo(outputFilePath);
                Console.WriteLine($"AppHost: File exists, size: {fileInfo.Length} bytes (poll #{pollCount})");
                if (fileInfo.Length > 0)
                {
                    // Additional small delay to ensure write is complete
                    await Task.Delay(200);
                    fileFound = true;
                    Console.WriteLine($"AppHost: File found with content after {pollCount} polls");
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"AppHost: IOException checking file: {ex.Message}");
                // File might still be locked, continue polling
            }
        }
        else
        {
            Console.WriteLine($"AppHost: File not found yet (poll #{pollCount})");
        }

        if (!fileFound)
        {
            await Task.Delay(100);
        }
    }

    if (!fileFound)
    {
        Console.WriteLine($"AppHost: File not found after 30 seconds of polling");
        return;
    }

    // Read and deserialize the result from the file
    ConsoleResult? result = null;
    try
    {
        if (File.Exists(outputFilePath))
        {
            Console.WriteLine($"AppHost: Reading file: {outputFilePath}");
            // Retry reading in case file is still being written
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    var jsonText = await File.ReadAllTextAsync(outputFilePath);
                    Console.WriteLine($"AppHost: Read {jsonText.Length} characters from file");
                    Console.WriteLine($"AppHost: JSON content: {jsonText}");
                    result = JsonSerializer.Deserialize<ConsoleResult>(jsonText);
                    Console.WriteLine($"AppHost: Deserialized result - Success: {result?.Success}, Output: {result?.Output}");
                    break;
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"AppHost: IOException reading file (attempt {i+1}): {ex.Message}");
                    // File might still be locked, wait and retry
                    await Task.Delay(200);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"AppHost: JSON deserialization failed: {ex.Message}");
                    // JSON deserialization failed
                    result = new ConsoleResult
                    {
                        Success = false,
                        Input = "Unknown",
                        Output = null,
                        ErrorMessage = $"Failed to deserialize result: {ex.Message}"
                    };
                    break;
                }
            }
        }
        else
        {
            Console.WriteLine($"AppHost: File does not exist: {outputFilePath}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"AppHost: Exception reading file: {ex.Message}");
        // Log error but continue
        result = new ConsoleResult
        {
            Success = false,
            Input = "Unknown",
            Output = null,
            ErrorMessage = $"Error reading output file: {ex.Message}"
        };
    }

    // Display the result via appropriate notification
    Console.WriteLine($"AppHost: Displaying notification, result is null: {result == null}");
    Console.WriteLine($"AppHost: Interaction service available: {interactionSvc.IsAvailable}");

    if (!interactionSvc.IsAvailable)
    {
        Console.WriteLine("AppHost: Interaction service not available, skipping notification");
        return;
    }

    if (result != null)
    {
        Console.WriteLine($"AppHost: Result.Success: {result.Success}");
        if (result.Success)
        {
            Console.WriteLine($"AppHost: Sending success notification with message: {result.Output ?? "Operation completed successfully."}");
            // Success notification with output
            await interactionSvc.PromptNotificationAsync(
                title: "Console App Success",
                message: result.Output ?? "Operation completed successfully.",
                options: new NotificationInteractionOptions
                {
                    Intent = MessageIntent.Success,
                    ShowSecondaryButton = false
                });
        }
        else
        {
            Console.WriteLine($"AppHost: Sending error notification with message: {result.ErrorMessage ?? "An unknown error occurred."}");
            // Failure notification with error message
            await interactionSvc.PromptNotificationAsync(
                title: "Console App Error",
                message: result.ErrorMessage ?? "An unknown error occurred.",
                options: new NotificationInteractionOptions
                {
                    Intent = MessageIntent.Error,
                    ShowSecondaryButton = false
                });
        }
    }
    else
    {
        Console.WriteLine("AppHost: Sending completion notification (no result)");
        // If no result captured, show a notification indicating completion but no result
        await interactionSvc.PromptNotificationAsync(
            title: "Console App Completed",
            message: "The console app has completed execution, but no result was captured from the file.",
            options: new NotificationInteractionOptions
            {
                Intent = MessageIntent.Information,
                ShowSecondaryButton = false
            });
    }
}
