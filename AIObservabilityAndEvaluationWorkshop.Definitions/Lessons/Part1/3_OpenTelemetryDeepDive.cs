using System.Diagnostics;
using System.Diagnostics.Metrics;
using JetBrains.Annotations;
using OpenTelemetry.Trace;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(1, 3, "OpenTelemetry Deep Dive", needsInput: false)]
public class OpenTelemetryDeepDive : LessonBase
{
    private readonly ActivitySource _activitySource;
    private readonly Meter _meter;
    
    // Metrics
    private readonly Counter<long> _requestCounter;
    private readonly Histogram<double> _processingTimeHistogram;
    private readonly UpDownCounter<long> _activeRequestsCounter;
    private long _simulatedQueueLength;

    public OpenTelemetryDeepDive()
    {
        _activitySource = new ActivitySource(typeof(OpenTelemetryDeepDive).FullName!);
        _meter = new Meter(typeof(OpenTelemetryDeepDive).FullName!);

        _requestCounter = _meter.CreateCounter<long>("workshop.requests.total", description: "Total number of requests processed in the deep dive");
        _processingTimeHistogram = _meter.CreateHistogram<double>("workshop.requests.duration", "ms", "Processing time for requests");
        _activeRequestsCounter = _meter.CreateUpDownCounter<long>("workshop.requests.active", description: "Number of currently active requests");
        
        _meter.CreateObservableGauge("workshop.queue.length", () => _simulatedQueueLength, description: "Simulated queue length");
    }

    protected override async Task<string> RunAsync(string message)
    {
        _requestCounter.Add(1, new TagList { { "message.length", message.Length } });
        _activeRequestsCounter.Add(1);
        _simulatedQueueLength++;

        Stopwatch sw = Stopwatch.StartNew();
        
        try
        {
            using Activity? activity = _activitySource.StartActivity("DeepDive.ProcessMessage");
            activity?.SetTag("message.content", message);
            activity?.AddEvent(new ActivityEvent("Message processing started"));

            // Simulate Step 1: Validation
            await Task.Delay(100);
            await RunStepWithActivityAsync("ValidateInput", async () =>
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    throw new ArgumentException("Message cannot be empty");
                }
                await Task.Delay(50);
                activity?.AddEvent(new ActivityEvent("Input validated"));
            });

            // Simulate Step 2: Enrichment
            await RunStepWithActivityAsync("EnrichData", async () =>
            {
                await Task.Delay(200);
                activity?.SetTag("enrichment.status", "complete");
                activity?.AddEvent(new ActivityEvent("Data enriched", tags: new ActivityTagsCollection { { "enrichment.provider", "MockProvider" } }));
            });

            // Simulate Step 3: AI Processing (mocked)
            await RunStepWithActivityAsync("AIProcessing", async () =>
            {
                using Activity? innerActivity = _activitySource.StartActivity("LargeLanguageModel.Inference");
                innerActivity?.SetTag("model.name", "gpt-4o");
                innerActivity?.SetTag("tokens.prompt", message.Length / 4);
                
                await Task.Delay(500); // Simulate LLM latency
                
                innerActivity?.SetTag("tokens.completion", 150);
                innerActivity?.AddEvent(new ActivityEvent("Inference completed"));
            });

            activity?.AddEvent(new ActivityEvent("Message processing finished"));
            
            return $"Deep Dive Complete! Processed message: '{message}' with full OTel instrumentation.";
        }
        finally
        {
            sw.Stop();
            _processingTimeHistogram.Record(sw.Elapsed.TotalMilliseconds);
            _activeRequestsCounter.Add(-1);
            _simulatedQueueLength--;
        }
    }

    private async Task RunStepWithActivityAsync(string stepName, Func<Task> action)
    {
        using Activity? activity = _activitySource.StartActivity(stepName);
        activity?.SetTag("step.name", stepName);
        try
        {
            await action();
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}
