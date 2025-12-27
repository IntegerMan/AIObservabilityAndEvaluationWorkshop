using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions;

public abstract class LessonBase
{
    public virtual string DisplayName => GetLessonAttribute().DisplayName;
    public virtual bool NeedsInput => GetLessonAttribute().NeedsInput;
    public virtual string? InputPromptTitle => GetLessonAttribute().InputPromptTitle;
    public virtual string? InputPromptMessage => GetLessonAttribute().InputPromptMessage;
    public virtual string? InformationalScreenTitle => GetLessonAttribute().InformationalScreenTitle;
    public virtual string? InformationalScreenMessage => GetLessonAttribute().InformationalScreenMessage;

    private readonly ActivitySource _activitySource;

    protected LessonBase()
    {
        _activitySource = new ActivitySource(GetType().FullName!);
    }

    public async Task<ConsoleResult> ExecuteAsync(string message, ILogger logger)
    {
        using Activity? activity = _activitySource.StartActivity($"Executing Lesson {DisplayName}", ActivityKind.Internal);
        
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
    
    protected LessonAttribute GetLessonAttribute() => 
        GetType().GetCustomAttribute<LessonAttribute>() ?? throw new InvalidOperationException("LessonAttribute is required on lessons.");
}