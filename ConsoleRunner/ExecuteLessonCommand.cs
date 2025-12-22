using AIObservabilityAndEvaluationWorkshop.Definitions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AIObservabilityAndEvaluationWorkshop.ConsoleRunner;

public class ExecuteLessonCommand(IServiceProvider serviceProvider, ILogger<ExecuteLessonCommand> logger)
{
    private readonly ActivitySource _activitySource = new(typeof(ExecuteLessonCommand).FullName!);

    public async Task ExecuteAsync(string message, string lessonDisplayName)
    {
        using Activity? activity = _activitySource.StartActivity(nameof(ExecuteLessonCommand));
        activity?.SetTag("lesson.display_name", lessonDisplayName);
        activity?.SetTag("input.message", message);

        logger.LogInformation("Executing lesson: {DisplayName} with message: {Message}", lessonDisplayName, message);

        IEnumerable<LessonBase> lessons = serviceProvider.GetServices<LessonBase>();
        LessonBase? lesson = lessons.FirstOrDefault(l => l.DisplayName.Equals(lessonDisplayName, StringComparison.OrdinalIgnoreCase));

        if (lesson == null)
        {
            logger.LogError("Lesson with display name {DisplayName} not found.", lessonDisplayName);
            
            AspireService aspireService = new(_activitySource, logger, message);
            aspireService.LogError(
                $"Lesson with display name {lessonDisplayName} not found.",
                lessonDisplayName,
                ("message", message),
                ("lesson.display_name", lessonDisplayName),
                ("command.name", "execute-lesson")
            );
            return;
        }

        ConsoleResult result = await lesson.ExecuteAsync(message, serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(lesson.GetType()));

        // Log the console result using the helper
        AspireService resultAspireService = new(_activitySource, logger, message);
        resultAspireService.LogResult(
            result,
            ("message", message),
            ("lesson.display_name", lessonDisplayName),
            ("command.name", "execute-lesson")
        );
    }
}
