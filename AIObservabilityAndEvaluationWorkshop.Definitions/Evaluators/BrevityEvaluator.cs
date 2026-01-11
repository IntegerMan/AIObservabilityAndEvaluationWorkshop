using System.ComponentModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Evaluators;

/// <summary>
/// An evaluator that grades responses by their brevity relative to a target length.
/// </summary>
/// <remarks>
/// This evaluator was ported from my JAIMES AF project (https://github.com/IntegerMan/JAIMES-AF) and adapted for demo purposes.
/// </remarks>
[Description("Grades responses based on their length relative to a target character count.")]
public class BrevityEvaluator : IEvaluator
{
    public const string BrevityMetricName = "Brevity";

    public int TargetCharacters { get; init; } = 140;
    public int Margin { get; init; } = 30;

    public IReadOnlyCollection<string> EvaluationMetricNames => [BrevityMetricName];

    public ValueTask<EvaluationResult> EvaluateAsync(
        IEnumerable<ChatMessage> messages,
        ChatResponse modelResponse,
        ChatConfiguration? chatConfiguration = null,
        IEnumerable<EvaluationContext>? evaluationContext = null,
        CancellationToken cancellationToken = default)
    {
        string text = modelResponse.Text ?? string.Empty;
        int charCount = text.Length;
        int wordCount = text.Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;

        if (Margin <= 0)
        {
            throw new InvalidOperationException("Brevity margin must be greater than zero.");
        }

        int score;
        if (Math.Abs(charCount - TargetCharacters) <= Margin)
        {
            score = 5;
        } else
        {
            // Deduct 1 per margin quantity over or under
            int deviation = Math.Abs(charCount - TargetCharacters) - Margin;
            int deduction = (int)Math.Ceiling((double)deviation / Margin);
            score = Math.Max(1, 5 - deduction);
        }

        string reasoning = score switch
        {
            5 => "The response length is ideal for a game master's reply.",
            4 => "The response length is acceptable, though it deviates slightly from the preferred length.",
            3 => "The response length is noticeably different from the target length.",
            2 => "The response length deviates significantly from the desired brevity.",
            _ => "The response length is poorly suited for the intended context."
        };

        EvaluationRating rating = score switch
        {
            >= 5 => EvaluationRating.Exceptional,
            4 => EvaluationRating.Good,
            3 or < 3 => EvaluationRating.Poor,
        };

        NumericMetric metric = new(BrevityMetricName)
        {
            Value = score,
            Reason = reasoning,
            Interpretation = new EvaluationMetricInterpretation(rating)
        };

        // Add additional information as diagnostics for this metric specifically
        metric.Diagnostics ??= [];
        metric.Diagnostics.Add(new EvaluationDiagnostic(
            EvaluationDiagnosticSeverity.Informational,
            $"Brevity Detail: {charCount} characters, {wordCount} words. Target: {TargetCharacters} (+/- {Margin})"));

        EvaluationResult result = new([metric]);

        // Add additional information as metadata
        result.AddOrUpdateMetadataInAllMetrics("Brevity.CharacterCount", charCount.ToString());
        result.AddOrUpdateMetadataInAllMetrics("Brevity.WordCount", wordCount.ToString());
        result.AddOrUpdateMetadataInAllMetrics("Brevity.TargetCharacters", TargetCharacters.ToString());
        result.AddOrUpdateMetadataInAllMetrics("Brevity.Margin", Margin.ToString());

        return new ValueTask<EvaluationResult>(result);
    }
}

