using JetBrains.Annotations;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(3, 4, "EXERCISE 4", needsInput: true,
    informationalScreenTitle: "Exercise 4",
    informationalScreenMessage: "This is an exercise lesson. Apply what you've learned about report generation, multiple scenarios, and trends in the previous lessons to complete this exercise.",
    inputPromptTitle: "Exercise 4 - Input",
    inputPromptMessage: "Enter your input for Exercise 4:")]
public class Exercise4Lesson : LessonBase
{
    // NOTE: If Matt and Victor haven't talked about unit / integration tests around evaluators at this point, tell them to SLOW THEIR ROLL AND SHOW YOU THE TESTS!

    protected override Task<string> RunAsync(string message)
    {
        /*
         * Your task in this final lesson is to generate an evaluation report in HTML or JSON format, saved locally on disk, that contains the results of your evaluations Exercise 2 or Exercise 3.
         * I'd encourage you to use multiple scenarios and/or multiple scenario runs to demonstrate the capabilities of the reporting system.
         */

        return Task.FromResult($"Exercise 4 placeholder. Input: {message}");
    }
}
