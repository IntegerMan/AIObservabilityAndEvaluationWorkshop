namespace AIObservabilityAndEvaluationWorkshop.Definitions;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class LessonAttribute(
    int part,
    int order,
    string displayName,
    bool needsInput = true,
    string? inputPromptTitle = null,
    string? inputPromptMessage = null,
    string? informationalScreenTitle = null,
    string? informationalScreenMessage = null) : Attribute
{
    public int Part { get; } = part;
    public int Order { get; } = order;
    public string DisplayName { get; } = displayName;
    public bool NeedsInput { get; } = needsInput;
    public string? InputPromptTitle { get; } = inputPromptTitle;
    public string? InputPromptMessage { get; } = inputPromptMessage;
    public string? InformationalScreenTitle { get; } = informationalScreenTitle;
    public string? InformationalScreenMessage { get; } = informationalScreenMessage;
}
