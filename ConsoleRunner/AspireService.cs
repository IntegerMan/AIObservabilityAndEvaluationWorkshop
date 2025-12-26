using System.Diagnostics;
using System.Text.Json;
using AIObservabilityAndEvaluationWorkshop.Definitions;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.ConsoleRunner;

public class AspireService(ActivitySource activitySource, ILogger logger, string? input)
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    public void LogResult(
        ConsoleResult result,
        params (string Key, object? Value)[] activityTags)
    {
        // Create a named activity for better trace visibility
        using Activity? activity = activitySource.StartActivity("Report to Aspire", ActivityKind.Producer);
        activity?.SetTag("output", result.Output);
        activity?.SetTag("error", result.ErrorMessage);

        // Set activity tags
        if (activity is not null && activityTags.Length > 0)
        {
            foreach ((string key, object? value) in activityTags)
            {
                activity.SetTag(key, value);
            }
        }

        // Log the result
        activity?.SetTag("success", result.Success);
        if (result.Success)
        {
            logger.LogInformation("Console operation successful. Input: {Input}, Output: {Output}", result.Input, result.Output);
        }
        else
        {
            logger.LogError("Console operation failed. Input: {Input}, Error: {ErrorMessage}", result.Input, result.ErrorMessage);
            activity?.SetStatus(ActivityStatusCode.Error, result.ErrorMessage);
        }
        
        // Serialize the result
        string json = JsonSerializer.Serialize(result);
        
        // Write to a temporary file to avoid console buffer truncation issues
        // Then output a short trigger string that the watcher can capture
        string tempFile = Path.GetTempFileName();
        string outputFile = Path.ChangeExtension(tempFile, ".json");
        
        try
        {
            // Delete the temp file created by GetTempFileName and create our .json file
            File.Delete(tempFile);
            File.WriteAllText(outputFile, json);
            logger.LogDebug("Wrote console result to file: {OutputFile}", outputFile);
            
            // Output a short trigger string (< 240 chars) that the watcher can capture
            // The file path should be well under 240 characters
            Console.WriteLine($"CONSOLE_RESULT_FILE: {outputFile}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write console result to file: {OutputFile}", outputFile);
            // Fall back to console output (may be truncated for large outputs)
            Console.WriteLine($"CONSOLE_RESULT: {json}");
        }

        // Log indented version for readability
        logger.LogInformation("Console result output: {Json}", JsonSerializer.Serialize(result, _jsonSerializerOptions));
    }

    public void LogSuccess(string? output, string? lessonId, string activityName = "AspireService.LogSuccess", params (string Key, object? Value)[] activityTags)
    {
        ConsoleResult result = new()
        {
            Success = true,
            Input = input,
            Output = output,
            ErrorMessage = null,
            LessonId = lessonId
        };

        LogResult(result, activityTags);
    }

    public void LogError(string? errorMessage, string? lessonId, params (string Key, object? Value)[] activityTags)
    {
        ConsoleResult result = new()
        {
            Success = false,
            Input = input,
            Output = null,
            ErrorMessage = errorMessage,
            LessonId = lessonId
        };

        LogResult(result, activityTags);
    }
}
