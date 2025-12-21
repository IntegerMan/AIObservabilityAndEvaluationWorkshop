using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AIObservabilityAndEvaluationWorkshop.Definitions;

public abstract class LessonBase
{
    public abstract string Id { get; }
    public abstract string DisplayName { get; }

    private static readonly ActivitySource ActivitySource = new("AIObservabilityAndEvaluationWorkshop.Lessons");

    public async Task<ConsoleResult> ExecuteAsync(string message, ILogger logger)
    {
        using var activity = ActivitySource.StartActivity($"Lesson.{Id}.Execute");
        if (activity == null && Activity.Current != null)
        {
            // If StartActivity returned null (e.g. no listeners), but there is a current activity,
            // it means we might want to manually create one if we want to ensure nesting, 
            // but usually StartActivity is what we want. 
            // In .NET, if a listener is active, it will automatically parent to Activity.Current.
        }
        
        activity?.SetTag("lesson.id", Id);
        activity?.SetTag("lesson.display_name", DisplayName);
        activity?.SetTag("input.message", message);

        logger.LogInformation("Starting lesson {LessonId}: {DisplayName}", Id, DisplayName);

        try
        {
            var output = await RunAsync(message);
            
            var result = new ConsoleResult
            {
                Success = true,
                Output = output,
                Input = message,
                LessonId = Id
            };

            logger.LogInformation("Lesson {LessonId} completed successfully", Id);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing lesson {LessonId}", Id);
            
            return new ConsoleResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Input = message,
                LessonId = Id
            };
        }
    }

    protected abstract Task<string> RunAsync(string message);
}
