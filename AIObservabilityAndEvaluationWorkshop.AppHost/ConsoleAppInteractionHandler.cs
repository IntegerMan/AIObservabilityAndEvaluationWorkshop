#pragma warning disable ASPIREINTERACTION001 // Interaction Service is for evaluation purposes only

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using AIObservabilityAndEvaluationWorkshop.Definitions;
using Microsoft.Extensions.DependencyInjection;

namespace AIObservabilityAndEvaluationWorkshop.AppHost;

/// <summary>
/// Handles user interaction and output watching for the console app resource.
/// </summary>
public class ConsoleAppInteractionHandler
{
    private readonly List<(Type Type, LessonAttribute Attribute)> _lessons;
    private string[] _appArgs = [];

    public ConsoleAppInteractionHandler()
    {
        // Get the assembly where LessonBase is defined
        Assembly assembly = typeof(LessonBase).Assembly;

        // Discover lessons and their metadata via reflection
        _lessons = assembly.GetTypes()
            .Select(t => (Type: t, Attribute: t.GetCustomAttribute<LessonAttribute>()))
            .Where(x => x.Attribute != null)
            .OrderBy(x => x.Attribute!.Part)
            .ThenBy(x => x.Attribute!.Order)
            .ToList()!;
    }

    /// <summary>
    /// Handles the output watcher match event, reading JSON from file and displaying results.
    /// </summary>
    public async Task HandleOutputMatchAsync(
        IServiceProvider serviceProvider,
        IReadOnlyDictionary<string, object> properties,
        string resourceName,
        string key,
        string message,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"AppHost: OnMatched event fired for {resourceName}: {key}, {message}");

        // Get the captured file path from the regex match
        if (!properties.TryGetValue("filepath", out object? filePathValue))
        {
            Console.WriteLine("AppHost: No file path captured from regex match");
            return;
        }

        string filePath = filePathValue.ToString()!;
        Console.WriteLine($"AppHost: Captured file path: {filePath}");

        string? json = await ReadJsonFromFileAsync(filePath, cancellationToken);
        if (json == null)
        {
            Console.WriteLine($"AppHost: Failed to read JSON from file: {filePath}");
            return;
        }

        Console.WriteLine($"AppHost: Read JSON from file, length: {json.Length} characters");

        try
        {
            // Deserialize the JSON result
            ConsoleResult? result = JsonSerializer.Deserialize<ConsoleResult>(json);

            // Clean up the temporary file
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AppHost: Warning - Failed to delete temp file: {ex.Message}");
            }

            if (result != null)
            {
                await DisplayResultAsync(serviceProvider, result, cancellationToken);
            }
            else
            {
                Console.WriteLine("AppHost: Failed to deserialize console result");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"AppHost: JSON deserialization failed: {ex.Message}");
            Console.WriteLine($"AppHost: JSON length was: {json.Length} characters");
            Console.WriteLine($"AppHost: JSON ends with: {json.Substring(Math.Max(0, json.Length - 100))}");
        }
    }

    /// <summary>
    /// Handles the start-with-input command, prompting for user input and starting the console app.
    /// </summary>
    public async Task<ExecuteCommandResult> HandleStartWithInputCommandAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken,
        string resourceName)
    {
        IInteractionService interactionService = serviceProvider.GetRequiredService<IInteractionService>();

        if (!interactionService.IsAvailable)
        {
            return new ExecuteCommandResult { Success = false, ErrorMessage = "Interaction service not available" };
        }

        // Prompt the user for lesson choice
        KeyValuePair<string, string>[] options = _lessons.Select(l =>
            new KeyValuePair<string, string>(l.Attribute!.DisplayName, $"{l.Attribute!.Part}.{l.Attribute!.Order} - {l.Attribute!.DisplayName}")).ToArray();

        if (options.Length == 0)
        {
            return new ExecuteCommandResult { Success = false, ErrorMessage = "No lessons found via discovery." };
        }

        InteractionResult<InteractionInput> lessonResult = await interactionService.PromptInputAsync(
            title: "Lesson Selection",
            message: "Please select a lesson:",
            input: new InteractionInput
            {
                Name = "DisplayName",
                InputType = InputType.Choice,
                Required = true,
                Options = options
            });

        if (lessonResult.Canceled)
        {
            return new ExecuteCommandResult { Success = false, ErrorMessage = "User cancelled lesson selection" };
        }

        string displayName = lessonResult.Data?.Value ?? options[0].Key;
        var selectedLessonInfo = _lessons.First(l => l.Attribute!.DisplayName == displayName);
        LessonAttribute selectedLesson = selectedLessonInfo.Attribute!;

        string message = "No Input needed";
        if (selectedLesson.NeedsInput)
        {
            // Show informational screen if provided
            if (!string.IsNullOrWhiteSpace(selectedLesson.InformationalScreenTitle) ||
                !string.IsNullOrWhiteSpace(selectedLesson.InformationalScreenMessage))
            {
                await interactionService.PromptMessageBoxAsync(
                    title: selectedLesson.InformationalScreenTitle ?? "Information",
                    message: selectedLesson.InformationalScreenMessage ?? "",
                    options: new MessageBoxInteractionOptions
                    {
                        Intent = MessageIntent.Information,
                        EnableMessageMarkdown = true,
                        PrimaryButtonText = "Continue"
                    },
                    cancellationToken: cancellationToken);
            }

            // Prompt the user for message with custom title/message if provided
            string inputTitle = selectedLesson.InputPromptTitle ?? "Message Input";
            string inputMessage = selectedLesson.InputPromptMessage ?? "Please enter your message:";

            InteractionResult<InteractionInput> messageResult = await interactionService.PromptInputAsync(
                title: "Provide Input",
                message: inputMessage,
                input: new InteractionInput
                {
                    Name = inputTitle,
                    InputType = InputType.Text,
                    Required = true,
                    Placeholder = "Enter your message here"
                });

            if (messageResult.Canceled)
            {
                return new ExecuteCommandResult { Success = false, ErrorMessage = "User cancelled message input" };
            }

            message = messageResult.Data?.Value ?? "Hello, World!";
        }

        _appArgs = ["execute", message, displayName];

        Console.WriteLine($"AppHost: Starting console app with args: {string.Join(", ", _appArgs)}");

        // Simply start the resource
        ResourceCommandService commandService = serviceProvider.GetRequiredService<ResourceCommandService>();
        return await commandService.ExecuteCommandAsync(resourceName, "resource-start");
    }

    /// <summary>
    /// Configures the arguments for the console app.
    /// </summary>
    public void ConfigureArgs(IList<object> args)
    {
        args.Clear();
        foreach (string arg in _appArgs)
        {
            args.Add((object)arg);
        }
    }

    private async Task<string?> ReadJsonFromFileAsync(string filePath, CancellationToken cancellationToken)
    {
        int retryCount = 0;
        const int maxRetries = 10;
        const int retryDelayMs = 100;

        // Poll for file creation and handle file locking gracefully
        while (retryCount < maxRetries)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // Wait additional 200ms after detecting file to ensure write completion
                    await Task.Delay(200, cancellationToken);

                    // Try to read the file with retry for file locking
                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {
                            return await File.ReadAllTextAsync(filePath, cancellationToken);
                        }
                        catch (IOException) when (i < 4)
                        {
                            // File might be locked, wait and retry
                            await Task.Delay(50, cancellationToken);
                        }
                    }
                }
                else
                {
                    // File doesn't exist yet, wait and retry
                    await Task.Delay(retryDelayMs, cancellationToken);
                    retryCount++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AppHost: Error reading file (attempt {retryCount + 1}): {ex.Message}");
                if (retryCount >= maxRetries - 1)
                {
                    Console.WriteLine($"AppHost: Failed to read file after {maxRetries} attempts");
                    return null;
                }
                await Task.Delay(retryDelayMs, cancellationToken);
                retryCount++;
            }
        }

        return null;
    }

    private async Task DisplayResultAsync(
        IServiceProvider serviceProvider,
        ConsoleResult result,
        CancellationToken cancellationToken)
    {
        // Get interaction service to show notification
        IInteractionService? interactionSvc = serviceProvider.GetService<IInteractionService>();

        if (interactionSvc is not { IsAvailable: true })
        {
            Console.WriteLine("AppHost: Interaction service not available, skipping notification");
            return;
        }

        string lessonTitlePrefix =
            !string.IsNullOrWhiteSpace(result.LessonId) ? $"{result.LessonId} " : "";

        switch (result.Success)
        {
            case true when !string.IsNullOrWhiteSpace(result.Output):
                await interactionSvc.PromptMessageBoxAsync(
                    title: $"{lessonTitlePrefix}Completed",
                    message: result.Output,
                    options: new MessageBoxInteractionOptions
                    {
                        Intent = MessageIntent.Success,
                        EnableMessageMarkdown = true,
                        PrimaryButtonText = "OK"
                    }, cancellationToken: cancellationToken);
                break;
            case true when string.IsNullOrWhiteSpace(result.Output):
                await interactionSvc.PromptMessageBoxAsync(
                    title: $"{lessonTitlePrefix}No Output",
                    message: "The operation completed, but produced no output",
                    options: new MessageBoxInteractionOptions
                    {
                        Intent = MessageIntent.Warning,
                        EnableMessageMarkdown = false,
                        PrimaryButtonText = "OK"
                    }, cancellationToken: cancellationToken);
                break;
            default:
                await interactionSvc.PromptMessageBoxAsync(
                    title: $"{lessonTitlePrefix}Error",
                    message: result.ErrorMessage ?? "An unknown error occurred.",
                    options: new MessageBoxInteractionOptions
                    {
                        Intent = MessageIntent.Error,
                        EnableMessageMarkdown = false,
                        PrimaryButtonText = "OK"
                    }, cancellationToken: cancellationToken);
                break;
        }
    }
}

