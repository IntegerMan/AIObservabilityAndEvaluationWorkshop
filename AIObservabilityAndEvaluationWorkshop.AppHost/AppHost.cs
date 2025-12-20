#pragma warning disable ASPIREINTERACTION001 // Interaction Service is for evaluation purposes only

using Microsoft.Extensions.DependencyInjection;
using Projects;
using System.Text.Json;
using AIObservabilityAndEvaluationWorkshop.Definitions;

var builder = DistributedApplication.CreateBuilder(args);

// Add the console app project (without WithExplicitStart so it doesn't auto-start)
IResourceBuilder<ProjectResource> consoleAppBuilder =
    builder.AddProject<AIObservabilityAndEvaluationWorkshop_ConsoleRunner>("console-app")
        //.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:19288")
        .WithExplicitStart();

// Subscribe to AfterResourcesCreatedEvent to prompt for input after dashboard is ready
builder.Eventing.Subscribe<AfterResourcesCreatedEvent>(async (@event, cancellationToken) =>
{
    var interactionService = @event.Services.GetRequiredService<IInteractionService>();

    if (!interactionService.IsAvailable)
    {
        return;
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
        await interactionService.PromptNotificationAsync("Cancelled", "User canceled the input.");
        return;
    }

    var message = result.Data?.Value ?? "Hello, World!";

    // Ensure the console app has the OTLP endpoint for telemetry
    //consoleAppBuilder = consoleAppBuilder.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:19288");

    // Configure the console app to run with the display command and message
    consoleAppBuilder.WithArgs("display", message);

    // Show a notification with the message
    await interactionService.PromptNotificationAsync("Input Received",
        $"You entered: '{message}'. The app can now be started from the dashboard.", new NotificationInteractionOptions()
        {
            Intent = MessageIntent.Information,
            ShowSecondaryButton = false,
        });
});

// Subscribe to console app resource ready event to capture and display output after completion
consoleAppBuilder.OnResourceReady(async (resource, readyEvent, cancellationToken) =>
{
    var interactionService = readyEvent.Services.GetRequiredService<IInteractionService>();
    
    if (!interactionService.IsAvailable)
    {
        return;
    }
    
    // The console app writes output to console_output.txt in its base directory (AppContext.BaseDirectory)
    // In Aspire, project resources run in their build output directory
    // We need to construct the path to the console app's output file
    
    // Construct the path based on the known project structure
    // The console app is in the ConsoleRunner directory relative to the solution root
    var appHostAssemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
    var appHostDir = Path.GetDirectoryName(appHostAssemblyLocation) ?? AppContext.BaseDirectory;
    
    // Navigate from AppHost/bin/Debug/net10.0 to solution root, then to ConsoleRunner/bin/Debug/net10.0
    var solutionRoot = Path.GetFullPath(Path.Combine(appHostDir, "..", "..", "..", ".."));
    
    // Try Debug first, then Release
    var debugPath = Path.Combine(solutionRoot, "ConsoleRunner", "bin", "Debug", "net10.0", "console_output.json");
    var releasePath = Path.Combine(solutionRoot, "ConsoleRunner", "bin", "Release", "net10.0", "console_output.json");
    
    // Also try relative to AppHost directory (in case paths are different)
    var relativeDebugPath = Path.Combine(appHostDir, "..", "..", "..", "ConsoleRunner", "bin", "Debug", "net10.0", "console_output.json");
    var relativeReleasePath = Path.Combine(appHostDir, "..", "..", "..", "ConsoleRunner", "bin", "Release", "net10.0", "console_output.json");

    // Find the first existing file, or use Debug as default
    var outputFilePath = File.Exists(debugPath) ? debugPath :
                        File.Exists(releasePath) ? releasePath :
                        File.Exists(relativeDebugPath) ? Path.GetFullPath(relativeDebugPath) :
                        File.Exists(relativeReleasePath) ? Path.GetFullPath(relativeReleasePath) :
                        debugPath; // Default to Debug path for file watcher
    
    // Use FileSystemWatcher to monitor the output file
    var watchDirectory = Path.GetDirectoryName(outputFilePath);
    if (string.IsNullOrEmpty(watchDirectory) || !Directory.Exists(watchDirectory))
    {
        // If directory doesn't exist, wait a bit and check again
        await Task.Delay(1000, cancellationToken);
        watchDirectory = Path.GetDirectoryName(outputFilePath);
    }
    
    FileSystemWatcher? fileWatcher = null;
    if (!string.IsNullOrEmpty(watchDirectory) && Directory.Exists(watchDirectory))
    {
        fileWatcher = new FileSystemWatcher
        {
            Path = watchDirectory,
            Filter = Path.GetFileName(outputFilePath),
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents = true
        };
    }
    
    var fileUpdated = new TaskCompletionSource<bool>();
    var fileRead = false;
    
    // Watch for file changes if watcher was created
    if (fileWatcher != null)
    {
        fileWatcher.Changed += (_, e) =>
        {
            if (!fileRead && File.Exists(e.FullPath))
            {
                // Small delay to ensure file write is complete
                Task.Delay(100).ContinueWith(_ =>
                {
                    if (!fileRead)
                    {
                        fileRead = true;
                        fileUpdated.TrySetResult(true);
                    }
                });
            }
        };
        
        fileWatcher.Created += (_, _) =>
        {
            if (!fileRead)
            {
                fileRead = true;
                fileUpdated.TrySetResult(true);
            }
        };
    }
    
    // Also check if file already exists (console app might have completed quickly)
    if (File.Exists(outputFilePath))
    {
        await Task.Delay(500, cancellationToken); // Wait a bit for file to be fully written
        if (!fileRead)
        {
            fileRead = true;
            fileUpdated.TrySetResult(true);
        }
    }
    
    // Wait for file update or timeout after 10 seconds
    var timeoutTask = Task.Delay(10000, cancellationToken);
    await Task.WhenAny(fileUpdated.Task, timeoutTask);
    
    if (fileWatcher != null)
    {
        fileWatcher.EnableRaisingEvents = false;
        fileWatcher.Dispose();
    }
    
    // Read and deserialize the result from the file
    ConsoleResult? result = null;
    try
    {
        if (File.Exists(outputFilePath))
        {
            // Retry reading in case file is still being written
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    var jsonText = await File.ReadAllTextAsync(outputFilePath, cancellationToken);
                    result = JsonSerializer.Deserialize<ConsoleResult>(jsonText);
                    break;
                }
                catch (IOException)
                {
                    // File might still be locked, wait and retry
                    await Task.Delay(200, cancellationToken);
                }
                catch (JsonException ex)
                {
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
    }
    catch (Exception ex)
    {
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
    if (result != null)
    {
        if (result.Success)
        {
            // Success notification with output
            await interactionService.PromptNotificationAsync(
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
            // Failure notification with error message
            await interactionService.PromptNotificationAsync(
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
        // If no result captured, show a notification indicating completion but no result
        await interactionService.PromptNotificationAsync(
            title: "Console App Completed",
            message: "The console app has completed execution, but no result was captured from the file.",
            options: new NotificationInteractionOptions
            {
                Intent = MessageIntent.Information,
                ShowSecondaryButton = false
            });
    }
});

builder.Build().Run();