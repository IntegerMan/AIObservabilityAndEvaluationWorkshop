#pragma warning disable ASPIREINTERACTION001 // Interaction Service is for evaluation purposes only

using Microsoft.Extensions.DependencyInjection;

namespace AIObservabilityAndEvaluationWorkshop.AppHost;

/// <summary>
/// Extension methods for adding a feedback resource to the Aspire application.
/// </summary>
public static class FeedbackResourceExtensions
{
    private const string QrCodePath = "../Images/FeedbackQR.png";

    /// <summary>
    /// Adds a feedback resource that allows participants to provide workshop feedback.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the resource.</param>
    /// <returns>A resource builder for the feedback resource.</returns>
    public static IResourceBuilder<FeedbackResource> AddFeedbackResource(
        this IDistributedApplicationBuilder builder,
        string name)
    {
        var resource = new FeedbackResource(name);

        return builder.AddResource(resource)
            .WithInitialState(new CustomResourceSnapshot
            {
                ResourceType = "Workshop",
                State = new ResourceStateSnapshot("Ready", KnownResourceStateStyles.Success),
                Properties = []
            })
            .WithCommand(
                name: "provide-feedback",
                displayName: "Provide Feedback",
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

                    string message = BuildFeedbackMessage();

                    await interactionService.PromptMessageBoxAsync(
                        title: "Thank You for Attending!",
                        message: message,
                        options: new MessageBoxInteractionOptions
                        {
                            Intent = MessageIntent.Success,
                            EnableMessageMarkdown = true,
                            PrimaryButtonText = "Done"
                        },
                        cancellationToken: context.CancellationToken);

                    return new ExecuteCommandResult { Success = true };
                },
                commandOptions: new CommandOptions
                {
                    Description = "Provide feedback about the workshop",
                    IconName = "Chat",
                    IsHighlighted = true
                });
    }

    private static string BuildFeedbackMessage()
    {
        string qrCodeAbsolutePath = Path.GetFullPath(QrCodePath);
        bool qrCodeExists = File.Exists(qrCodeAbsolutePath);

        var messageBuilder = new System.Text.StringBuilder();

        messageBuilder.AppendLine("## We'd Love Your Feedback!");
        messageBuilder.AppendLine();
        messageBuilder.AppendLine(
            "Your feedback helps us improve future workshops and create better learning experiences.");
        messageBuilder.AppendLine();

        if (qrCodeExists)
        {
            messageBuilder.AppendLine("### Scan to Provide Feedback");
            messageBuilder.AppendLine();

            // Convert image to base64 data URL for inline embedding
            byte[] imageBytes = File.ReadAllBytes(qrCodeAbsolutePath);
            string base64 = Convert.ToBase64String(imageBytes);
            string dataUrl = $"data:image/png;base64,{base64}";
            messageBuilder.AppendLine($"![Feedback QR Code]({dataUrl})");
        }
        else
        {
            messageBuilder.AppendLine(
                $"> **Note**: QR code image not found. Please add `{qrCodeAbsolutePath}` to display the QR code.");
        }

        messageBuilder.AppendLine();
        messageBuilder.AppendLine("---");
        messageBuilder.AppendLine();
        messageBuilder.AppendLine("🙏 **Thank you for participating in the AI Observability & Evaluation Workshop!**");

        return messageBuilder.ToString();
    }
}
