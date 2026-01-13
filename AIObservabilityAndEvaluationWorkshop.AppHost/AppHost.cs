using AIObservabilityAndEvaluationWorkshop.AppHost;
using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// Add workshop content as parent resource - grouped resources appear first in dashboard
var workshopContent = builder.AddWelcomeResource("workshop-content");

var enableSensitiveDataLogging = builder.AddParameter("EnableSensitiveDataLogging", secret: false);

var aiProvider = builder.AddParameter("AIProvider", secret: false);
var aiModel = builder.AddParameter("AIModel", secret: false);
var aiEndpoint = builder.AddParameter("AIEndpoint", secret: false);
var AIFoundryProjectEndpoint = builder.AddParameter("AIFoundryProjectEndpoint", secret: false);
var aiKey = builder.AddParameter("AIKey", secret: true);
var aiUseIdentity = builder.AddParameter("AIUseAzureIdentity", secret: false);
var allowUntrustedCertificates = builder.AddParameter("AllowUntrustedCertificates", secret: false);
var evaluationResultsPath = builder.AddParameter("EvaluationResultsPath", secret: false);
var reportsPath = builder.AddParameter("ReportsPath", secret: false);
var reportStorageType = builder.AddParameter("ReportStorageType", secret: false);
var azureStorageEndpoint = builder.AddParameter("AzureStorageDataLakeEndpoint", secret: false);
var azureStorageContainer = builder.AddParameter("AzureStorageContainer", secret: false);

var ollama = builder.AddOllama("ollama")
    .WithDataVolume("ollama-data");
var llama = ollama.AddModel("llama3.2");

// Create interaction handler for console app
ConsoleAppInteractionHandler handler = new();

// Add the console app project as child of workshop-content
builder.AddProject<AIObservabilityAndEvaluationWorkshop_ConsoleRunner>("lessons")
    .WithParentRelationship(workshopContent)
    .WithReference(llama)
    .WithReference(ollama)
    .WithEnvironment("AI_MODEL", "llama3.2")
    .WithEnvironment("EnableSensitiveDataLogging", enableSensitiveDataLogging)
    .WithEnvironment("AIProvider", aiProvider)
    .WithEnvironment("AIModel", aiModel)
    .WithEnvironment("AIEndpoint", aiEndpoint)
    .WithEnvironment("AIFoundryProjectEndpoint", AIFoundryProjectEndpoint)
    .WithEnvironment("AIKey", aiKey)
    .WithEnvironment("AIUseIdentity", aiUseIdentity)
    .WithEnvironment("AllowUntrustedCertificates", allowUntrustedCertificates)
    .WithEnvironment("EvaluationResultsPath", evaluationResultsPath)
    .WithEnvironment("ReportsPath", reportsPath)
    .WithEnvironment("ReportStorageType", reportStorageType)
    .WithEnvironment("AzureStorageDataLakeEndpoint", azureStorageEndpoint)
    .WithEnvironment("AzureStorageContainer", azureStorageContainer)
    .WithExplicitStart()
    .WithArgs(context => handler.ConfigureArgs(context.Args))
    .WithOutputWatcher(ConsoleAppHelpers.GetConsoleResultRegex(), isSecret: false, "filepath")
    .OnMatched(async (e, ct) =>
    {
        // Extract typed properties from the event args
        IServiceProvider serviceProvider = e.ServiceProvider;
        var properties = e.Properties;
        string resourceName = e.Resource.Name;
        string key = e.Key;
        string message = e.Message;

        await handler.HandleOutputMatchAsync(serviceProvider, properties, resourceName, key, message, ct);
    })
    .WithCommand("start-with-input", "Start with Input", async context =>
        {
            // Extract typed properties from the command context
            IServiceProvider serviceProvider = context.ServiceProvider;
            CancellationToken cancellationToken = context.CancellationToken;
            string resourceName = context.ResourceName;

            return await handler.HandleStartWithInputCommandAsync(serviceProvider, cancellationToken, resourceName);
        },
        new CommandOptions
        {
            Description = "Configure the console app with user input and start it",
            IconName = "Play",
            IsHighlighted = true
        });

// Add the unit test project as child of workshop-content
builder.AddProject<AIObservabilityAndEvaluationWorkshop_Tests>("unit-tests")
    .WithParentRelationship(workshopContent)
    .WithReference(llama)
    .WithReference(ollama)
    .WithEnvironment("AI_MODEL", "llama3.2")
    .WithEnvironment("EnableSensitiveDataLogging", enableSensitiveDataLogging)
    .WithEnvironment("AIProvider", aiProvider)
    .WithEnvironment("AIModel", aiModel)
    .WithEnvironment("AIEndpoint", aiEndpoint)
    .WithEnvironment("AIFoundryProjectEndpoint", AIFoundryProjectEndpoint)
    .WithEnvironment("AIKey", aiKey)
    .WithEnvironment("AIUseIdentity", aiUseIdentity)
    .WithEnvironment("AllowUntrustedCertificates", allowUntrustedCertificates)
    .WithExplicitStart();

// Add feedback resource - standalone resources appear after grouped resources
builder.AddFeedbackResource("workshop-feedback");

builder.Build().Run();

