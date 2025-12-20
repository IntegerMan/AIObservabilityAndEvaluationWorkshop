namespace AIObservabilityAndEvaluationWorkshop.Definitions;

/// <summary>
/// Represents the result of a console operation with success/failure status and associated data.
/// </summary>
public record ConsoleResult
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// The error message, used only when Success is false.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// The output content, used only when Success is true.
    /// </summary>
    public string? Output { get; init; }

    /// <summary>
    /// The original input provided by the user.
    /// </summary>
    public string? Input { get; init; }
}