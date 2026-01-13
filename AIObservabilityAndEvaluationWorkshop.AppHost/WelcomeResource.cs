#pragma warning disable ASPIREINTERACTION001 // Interaction Service is for evaluation purposes only

using Aspire.Hosting.ApplicationModel;

namespace AIObservabilityAndEvaluationWorkshop.AppHost;

/// <summary>
/// A custom resource that displays welcome information to workshop participants.
/// This resource has no executable component - it exists solely to provide
/// an interactive panel in the Aspire dashboard.
/// </summary>
public sealed class WelcomeResource(string name) : Resource(name)
{
}
