using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace AIObservabilityAndEvaluationWorkshop.ConsoleRunner;

/// <summary>
/// Provides diagnostic logging for OpenTelemetry configuration and telemetry export status.
/// </summary>
public static class TelemetryDiagnostics
{
    /// <summary>
    /// Logs OTEL environment variables and configuration before services are configured.
    /// </summary>
    public static void LogPreConfigurationDiagnostics(HostApplicationBuilder builder)
    {
        try
        {
            // Create a temporary logger factory to log diagnostics before the host is built
            using ILoggerFactory tempLoggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
            ILogger logger = tempLoggerFactory.CreateLogger("TelemetryDiagnostics");
            
            logger.LogInformation("=== OpenTelemetry Configuration Diagnostics ===");
            
            // Log OTEL environment variables
            string? otelServiceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME");
            string? otelResourceAttributes = Environment.GetEnvironmentVariable("OTEL_RESOURCE_ATTRIBUTES");
            string? otelExporterOtlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
            
            logger.LogInformation("OTEL_SERVICE_NAME: {Value}", otelServiceName ?? "(NOT SET)");
            logger.LogInformation("OTEL_RESOURCE_ATTRIBUTES: {Value}", otelResourceAttributes ?? "(NOT SET)");
            logger.LogInformation("OTEL_EXPORTER_OTLP_ENDPOINT: {Value}", otelExporterOtlpEndpoint ?? "(NOT SET)");
            
            // Additional diagnostic information
            logger.LogInformation("Application Name: {AppName}", builder.Environment.ApplicationName);
            logger.LogInformation("Environment: {Environment}", builder.Environment.EnvironmentName);
            
            // Check if OTEL_EXPORTER_OTLP_ENDPOINT is set (required for Aspire telemetry)
            if (string.IsNullOrWhiteSpace(otelExporterOtlpEndpoint))
            {
                logger.LogWarning(
                    "OTEL_EXPORTER_OTLP_ENDPOINT is not set! Telemetry will NOT be exported to Aspire dashboard. " +
                    "Aspire should set this automatically. Check AppHost configuration.");
            }
            else
            {
                logger.LogInformation("OTLP exporter endpoint detected - telemetry should be exported to Aspire dashboard");
            }
            
            logger.LogInformation("=== End Diagnostics ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to log OTEL diagnostics: {ex.Message}");
        }
    }

    /// <summary>
    /// Logs telemetry configuration status after services are configured and host is built.
    /// </summary>
    public static void LogPostConfigurationDiagnostics(IHost host, HostApplicationBuilder builder)
    {
        ILogger logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(TelemetryDiagnostics));
        
        logger.LogInformation("=== Post-Configuration Telemetry Status ===");
        
        // Check OTLP endpoint configuration
        string? otlpEndpointAfterConfig = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (string.IsNullOrWhiteSpace(otlpEndpointAfterConfig))
        {
            // Try to get from environment variable as fallback
            otlpEndpointAfterConfig = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
        }
        
        logger.LogInformation("OTEL_EXPORTER_OTLP_ENDPOINT (from config): {Value}", 
            otlpEndpointAfterConfig ?? "(NOT SET - telemetry will NOT be exported)");
        
        if (string.IsNullOrWhiteSpace(otlpEndpointAfterConfig))
        {
            logger.LogWarning(
                "OTLP exporter is NOT configured. Telemetry will not reach Aspire dashboard. " +
                "Troubleshooting steps:\n" +
                "1. Verify Aspire AppHost is running and dashboard is accessible\n" +
                "2. Check that AddProject() is called correctly in AppHost.cs\n" +
                "3. Ensure Aspire.Hosting package is referenced in ConsoleRunner.csproj\n" +
                "4. Try explicitly setting OTEL_EXPORTER_OTLP_ENDPOINT in AppHost.cs using .WithEnvironment()\n" +
                "5. Check Aspire dashboard logs for connection errors");
        }
        else
        {
            logger.LogInformation("OTLP exporter should be active. Check Aspire dashboard for telemetry.");
        }
        
        // Check ActivitySource registration
        LogActivitySourceDiagnostics(logger, builder.Environment.ApplicationName);
        
        logger.LogInformation("=== End Post-Configuration Status ===");
    }

    /// <summary>
    /// Logs diagnostic information about ActivitySource registration and activity creation.
    /// </summary>
    private static void LogActivitySourceDiagnostics(ILogger logger, string applicationName)
    {
        logger.LogInformation("--- ActivitySource Diagnostics ---");
        logger.LogInformation("Registered ActivitySource name (from ApplicationName): {Name}", applicationName);
        
        // Check if ActivitySource is listening
        ActivitySource testSource = new(applicationName);
        bool isListening = testSource.HasListeners();
        logger.LogInformation("ActivitySource '{Name}' HasListeners: {IsListening}", applicationName, isListening);
        
        // Test creating an activity
        using Activity? testActivity = testSource.StartActivity("DiagnosticTest");
        if (testActivity != null)
        {
            logger.LogInformation("Successfully created test activity. Activity ID: {ActivityId}, TraceId: {TraceId}", 
                testActivity.Id, testActivity.TraceId);
            logger.LogInformation("Activity is being recorded: {IsRecorded}", testActivity.IsAllDataRequested);
        }
        else
        {
            logger.LogWarning(
                "Failed to create activity from ActivitySource '{Name}'. " +
                "This usually means the ActivitySource is not registered with OpenTelemetry. " +
                "Check that AddSource() is called in ConfigureOpenTelemetry() with the correct name.",
                applicationName);
        }
        
        // Check DisplayCommand ActivitySource
        string displayCommandSourceName = typeof(DisplayCommand).FullName!;
        ActivitySource displayCommandSource = new(displayCommandSourceName);
        bool displayCommandListening = displayCommandSource.HasListeners();
        logger.LogInformation("DisplayCommand ActivitySource '{Name}' HasListeners: {IsListening}", 
            displayCommandSourceName, displayCommandListening);
        
        if (!displayCommandListening)
        {
            logger.LogWarning(
                "DisplayCommand ActivitySource '{Name}' is NOT listening! " +
                "Activities created in DisplayCommand will NOT be exported. " +
                "Ensure this ActivitySource name is registered in ConfigureOpenTelemetry() using AddSource().",
                displayCommandSourceName);
        }
        else
        {
            using Activity? displayCommandActivity = displayCommandSource.StartActivity("DisplayCommandTest");
            if (displayCommandActivity != null)
            {
                logger.LogInformation("DisplayCommand ActivitySource can create activities. Activity will be exported.");
            }
        }
        
        logger.LogInformation("--- End ActivitySource Diagnostics ---");
    }
}

