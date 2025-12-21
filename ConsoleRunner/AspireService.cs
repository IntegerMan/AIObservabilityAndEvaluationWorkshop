using AIObservabilityAndEvaluationWorkshop.Definitions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace AIObservabilityAndEvaluationWorkshop.ConsoleRunner;

public class AspireService(ActivitySource activitySource, ILogger logger, string? input)
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    public void LogResult(
        ConsoleResult result,
        params (string Key, object? Value)[] activityTags)
    {
        // Create a named activity for better trace visibility
        using Activity? activity = activitySource.StartActivity("Report to Aspire");

        // Set activity tags
        if (activity is not null && activityTags.Length > 0)
        {
            foreach ((string key, object? value) in activityTags)
            {
                activity.SetTag(key, value);
            }
        }

        // Log the result
        if (result.Success)
        {
            logger.LogInformation("Console operation successful. Input: {Input}, Output: {Output}", result.Input, result.Output);
        }
        else
        {
            logger.LogError("Console operation failed. Input: {Input}, Error: {ErrorMessage}", result.Input, result.ErrorMessage);
        }


        // Serialize and output the result in a standardized format for AppHost to capture
        // Use single-line JSON for console output (regex capture works better with single-line)
        string json = JsonSerializer.Serialize(result);
        Console.WriteLine($"CONSOLE_RESULT: {json}");

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
