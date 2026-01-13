#pragma warning disable ASPIREINTERACTION001 // Interaction Service is for evaluation purposes only

using Microsoft.Extensions.DependencyInjection;

namespace AIObservabilityAndEvaluationWorkshop.AppHost;

/// <summary>
/// Extension methods for adding a welcome resource to the Aspire application.
/// </summary>
public static class WelcomeResourceExtensions
{
    private const string RepositoryUrl = "https://github.com/IntegerMan/AIObservabilityAndEvaluationWorkshop";
    private const string QrCodePath = "../Images/WelcomeQR.png";

    /// <summary>
    /// Adds a welcome resource that displays workshop information to participants.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the resource.</param>
    /// <returns>A resource builder for the welcome resource.</returns>
    public static IResourceBuilder<WelcomeResource> AddWelcomeResource(
        this IDistributedApplicationBuilder builder,
        string name)
    {
        var resource = new WelcomeResource(name);

        return builder.AddResource(resource)
            .WithInitialState(new CustomResourceSnapshot
            {
                ResourceType = "Workshop",
                State = new ResourceStateSnapshot("Ready", KnownResourceStateStyles.Success),
                Properties = []
            })
            .WithCommand(
                name: "view-welcome",
                displayName: "View Welcome",
                executeCommand: async context =>
                {
                    IInteractionService interactionService =
                        context.ServiceProvider.GetRequiredService<IInteractionService>();

                    if (!interactionService.IsAvailable)
                    {
                        return new ExecuteCommandResult
                        {
                            Success = false,
                            ErrorMessage = "Interaction service not available"
                        };
                    }

                    string message = BuildWelcomeMessage();

                    await interactionService.PromptMessageBoxAsync(
                        title: "Welcome to the AI Observability & Evaluation Workshop!",
                        message: message,
                        options: new MessageBoxInteractionOptions
                        {
                            Intent = MessageIntent.Information,
                            EnableMessageMarkdown = true,
                            PrimaryButtonText = "Let's Get Started!"
                        },
                        cancellationToken: context.CancellationToken);

                    return new ExecuteCommandResult { Success = true };
                },
                commandOptions: new CommandOptions
                {
                    Description = "View workshop welcome information and setup instructions",
                    IconName = "Home",
                    IsHighlighted = true
                });
    }

    private static string BuildWelcomeMessage()
    {
        string qrCodeAbsolutePath = Path.GetFullPath(QrCodePath);
        bool qrCodeExists = File.Exists(qrCodeAbsolutePath);

        var messageBuilder = new System.Text.StringBuilder();

        messageBuilder.AppendLine("## Getting Started");
        messageBuilder.AppendLine();
        messageBuilder.AppendLine($"📚 **Repository**: [{RepositoryUrl}]({RepositoryUrl})");
        messageBuilder.AppendLine();
        messageBuilder.AppendLine(
            "📖 Please read the **README.md** file for detailed setup instructions and workshop content.");
        messageBuilder.AppendLine();

        if (qrCodeExists)
        {
            messageBuilder.AppendLine("### Scan to Access Repository");
            messageBuilder.AppendLine();

            // Convert image to base64 data URL for inline embedding
            byte[] imageBytes = File.ReadAllBytes(qrCodeAbsolutePath);
            string base64 = Convert.ToBase64String(imageBytes);
            string dataUrl = $"data:image/png;base64,{base64}";
            messageBuilder.AppendLine($"![Repository QR Code]({dataUrl})");
        }
        else
        {
            messageBuilder.AppendLine(
                $"> **Note**: QR code image not found. Please add `{qrCodeAbsolutePath}` to display the QR code.");
        }

        return messageBuilder.ToString();
    }
}
