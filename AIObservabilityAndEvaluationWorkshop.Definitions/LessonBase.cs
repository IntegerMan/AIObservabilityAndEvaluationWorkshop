using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions;

public abstract class LessonBase
{
    public virtual string DisplayName => 
        GetType().GetCustomAttribute<LessonAttribute>()?.DisplayName ?? GetType().Name;

    public virtual bool NeedsInput => 
        GetType().GetCustomAttribute<LessonAttribute>()?.NeedsInput ?? true;

    private readonly ActivitySource _activitySource;

    protected LessonBase()
    {
        _activitySource = new ActivitySource(GetType().FullName!);
    }

    public async Task<ConsoleResult> ExecuteAsync(string message, ILogger logger)
    {
        using Activity? activity = _activitySource.StartActivity($"Executing Lesson {DisplayName}");
        
        activity?.SetTag("lesson.display_name", DisplayName);
        activity?.SetTag("input.message", message);

        logger.LogDebug("Starting lesson: {DisplayName}", DisplayName);

        try
        {
            string output = await RunAsync(message);
            
            ConsoleResult result = new()
            {
                Success = true,
                Output = output,
                Input = message,
                LessonId = DisplayName
            };

            logger.LogInformation("Lesson {DisplayName} completed successfully", DisplayName);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing lesson {DisplayName}", DisplayName);
            
            throw;
        }
    }

    protected abstract Task<string> RunAsync(string message);
}
