#pragma warning disable ASPIREINTERACTION001 // Interaction Service is for evaluation purposes only

using System.IO;
using System.Reflection;
using System.Text.Json;
using AIObservabilityAndEvaluationWorkshop.AppHost;
using AIObservabilityAndEvaluationWorkshop.Definitions;
using Microsoft.Extensions.DependencyInjection;
using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var enableSensitiveDataLogging = builder.AddParameter("EnableSensitiveDataLogging", secret: false);

var aiProvider = builder.AddParameter("AIProvider", secret: false);
var aiModel = builder.AddParameter("AIModel", secret: false);
var aiEndpoint = builder.AddParameter("AIEndpoint", secret: false);
var aiKey = builder.AddParameter("AIKey", secret: true);
var aiUseIdentity = builder.AddParameter("AIUseAzureIdentity", secret: false);
var allowUntrustedCertificates = builder.AddParameter("AllowUntrustedCertificates", secret: false);
var evaluationResultsPath = builder.AddParameter("EvaluationResultsPath", secret: false);
var reportsPath = builder.AddParameter("ReportsPath", secret: false);
var reportStorageType = builder.AddParameter("ReportStorageType", secret: false);
var azureStorageConnectionString = builder.AddParameter("AzureStorageConnectionString", secret: true);
var azureStorageContainer = builder.AddParameter("AzureStorageContainer", secret: false);

var ollama = builder.AddOllama("ollama")
    .WithDataVolume("ollama-data");
var llama = ollama.AddModel("llama3.2");

// Get the assembly where LessonBase is defined
Assembly assembly = typeof(LessonBase).Assembly;

// Discover lessons and their metadata via reflection
var lessons = assembly.GetTypes()
    .Select(t => new { Type = t, Attribute = t.GetCustomAttribute<LessonAttribute>() })
    .Where(x => x.Attribute != null)
    .OrderBy(x => x.Attribute!.Part)
    .ThenBy(x => x.Attribute!.Order)
    .ToList();

string[] appArgs = [];

// Add the console app project (without WithExplicitStart so it doesn't auto-start)
IResourceBuilder<ProjectResource> consoleAppBuilder =
    builder.AddProject<AIObservabilityAndEvaluationWorkshop_ConsoleRunner>("console-app")
        .WithReference(llama)
        .WithReference(ollama)
        .WithEnvironment("AI_MODEL", "llama3.2")
        .WithEnvironment("EnableSensitiveDataLogging", enableSensitiveDataLogging)
        .WithEnvironment("AIProvider", aiProvider)
        .WithEnvironment("AIModel", aiModel)
        .WithEnvironment("AIEndpoint", aiEndpoint)
        .WithEnvironment("AIKey", aiKey)
        .WithEnvironment("AIUseIdentity", aiUseIdentity)
        .WithEnvironment("AllowUntrustedCertificates", allowUntrustedCertificates)
        .WithEnvironment("EvaluationResultsPath", evaluationResultsPath)
        .WithEnvironment("ReportsPath", reportsPath)
        .WithEnvironment("ReportStorageType", reportStorageType)
        .WithEnvironment("AzureStorageConnectionString", azureStorageConnectionString)
        .WithEnvironment("AzureStorageContainer", azureStorageContainer)
        .WithExplicitStart()
        .WithOutputWatcher(ConsoleAppHelpers.GetConsoleResultRegex(), isSecret: false, "filepath")
        .OnMatched(async (e, ct) =>
        {
            Console.WriteLine($"AppHost: OnMatched event fired for {e.Resource.Name}: {e.Key}, {e.Message}");

            // Get the captured file path from the regex match
            if (e.Properties.TryGetValue("filepath", out object? filePathValue))
            {
                string filePath = filePathValue.ToString()!;
                Console.WriteLine($"AppHost: Captured file path: {filePath}");

                string? json = null;
                int retryCount = 0;
                const int maxRetries = 10;
                const int retryDelayMs = 100;

                // Poll for file creation and handle file locking gracefully
                while (retryCount < maxRetries && json == null)
                {
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            // Wait additional 200ms after detecting file to ensure write completion
                            await Task.Delay(200, ct);
                            
                            // Try to read the file with retry for file locking
                            for (int i = 0; i < 5; i++)
                            {
                                try
                                {
                                    json = await File.ReadAllTextAsync(filePath, ct);
                                    break;
                                }
                                catch (IOException) when (i < 4)
                                {
                                    // File might be locked, wait and retry
                                    await Task.Delay(50, ct);
                                }
                            }
                        }
                        else
                        {
                            // File doesn't exist yet, wait and retry
                            await Task.Delay(retryDelayMs, ct);
                            retryCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"AppHost: Error reading file (attempt {retryCount + 1}): {ex.Message}");
                        if (retryCount >= maxRetries - 1)
                        {
                            Console.WriteLine($"AppHost: Failed to read file after {maxRetries} attempts");
                            return;
                        }
                        await Task.Delay(retryDelayMs, ct);
                        retryCount++;
                    }
                }

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
                        // Get interaction service to show notification
                        IInteractionService? interactionSvc = e.ServiceProvider.GetService<IInteractionService>();

                        if (interactionSvc is { IsAvailable: true })
                        {
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
                                        }, cancellationToken: ct);
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
                                        }, cancellationToken: ct);
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
                                        }, cancellationToken: ct);
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("AppHost: Interaction service not available, skipping notification");
                        }
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
            else
            {
                Console.WriteLine("AppHost: No file path captured from regex match");
            }
        })
        .WithArgs(context =>
        {
            context.Args.Clear();
            foreach (string arg in appArgs)
            {
                context.Args.Add(arg);
            }
        });

// Add a custom command that prompts for input and starts the resource
consoleAppBuilder.WithCommand("start-with-input", "Start with Input", async context =>
    {
        IInteractionService interactionService = context.ServiceProvider.GetRequiredService<IInteractionService>();

        if (!interactionService.IsAvailable)
        {
            return new ExecuteCommandResult { Success = false, ErrorMessage = "Interaction service not available" };
        }

        // Prompt the user for lesson choice
        KeyValuePair<string, string>[] options = lessons.Select(l => 
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
        var selectedLessonInfo = lessons.First(l => l.Attribute!.DisplayName == displayName);
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
                        EnableMessageMarkdown = selectedLesson.InformationalScreenSupportsMarkdown,
                        PrimaryButtonText = "Continue"
                    },
                    cancellationToken: context.CancellationToken);
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

        appArgs = ["execute", message, displayName];

        Console.WriteLine($"AppHost: Starting console app with args: {string.Join(", ", appArgs)}");

        // Simply start the resource
        ResourceCommandService commandService = context.ServiceProvider.GetRequiredService<ResourceCommandService>();
        return await commandService.ExecuteCommandAsync(context.ResourceName, "resource-start");
    },
    new CommandOptions
    {
        Description = "Configure the console app with user input and start it",
        IconName = "Play",
        IsHighlighted = true
    });

builder.Build().Run();
