#pragma warning disable ASPIREINTERACTION001 // Interaction Service is for evaluation purposes only

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

var ollama = builder.AddOllama("ollama")
    .WithDataVolume("ollama-data");
var llama = ollama.AddModel("llama3.2");

// Get the assembly where LessonBase is defined
Assembly assembly = typeof(LessonBase).Assembly;

// Discover lessons and their metadata via reflection
var lessons = assembly.GetTypes()
    .Select(t => new { Type = t, Attribute = t.GetCustomAttribute<LessonAttribute>() })
    .Where(x => x.Attribute != null)
    .Select(x => x.Attribute!)
    .OrderBy(a => a.Part)
    .ThenBy(a => a.Order)
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
        .WithExplicitStart()
        .WithOutputWatcher(ConsoleAppHelpers.GetConsoleResultRegex(), isSecret: false, "json")
        .OnMatched(async (e, ct) =>
        {
            Console.WriteLine($"AppHost: OnMatched event fired for {e.Resource.Name}: {e.Key}, {e.Message}");

            // Get the captured JSON from the regex match
            if (e.Properties.TryGetValue("json", out object? jsonValue))
            {
                string json = jsonValue.ToString()!;
                Console.WriteLine($"AppHost: Captured JSON: {json}");

                try
                {
                    // Deserialize the JSON result
                    ConsoleResult? result = JsonSerializer.Deserialize<ConsoleResult>(json);
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
                }
            }
            else
            {
                Console.WriteLine("AppHost: No JSON captured from regex match");
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
            new KeyValuePair<string, string>(l.DisplayName, $"{l.Part}.{l.Order} - {l.DisplayName}")).ToArray();

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
        LessonAttribute selectedLesson = lessons.First(l => l.DisplayName == displayName);

        string message = "No Input needed";
        if (selectedLesson.NeedsInput)
        {
            // Prompt the user for message
            InteractionResult<InteractionInput> messageResult = await interactionService.PromptInputAsync(
                title: "Message Input",
                message: "Please enter your message:",
                input: new InteractionInput
                {
                    Name = "UserMessage",
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

        appArgs = ["execute-lesson", message, displayName];

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
