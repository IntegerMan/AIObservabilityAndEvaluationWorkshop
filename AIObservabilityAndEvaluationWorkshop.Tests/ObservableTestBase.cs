using System.Diagnostics;
using Microsoft.Extensions.AI.Evaluation;

namespace AIObservabilityAndEvaluationWorkshop.Tests;

/// <summary>
/// Base class for tests that provides automatic telemetry instrumentation.
/// Wraps test execution in OpenTelemetry activities for observability in Aspire.
/// </summary>
public abstract class ObservableTestBase : IDisposable
{
    private readonly ActivitySource _activitySource;
    private Activity? _testActivity;

    protected ObservableTestBase()
    {
        _activitySource = new ActivitySource(GetType().FullName!);
    }

    /// <summary>
    /// Starts the main test activity. Called automatically before test execution.
    /// </summary>
    protected void StartTest(string testName)
    {
        _testActivity = _activitySource.StartActivity($"Test: {testName}", ActivityKind.Internal);
        _testActivity?.SetTag("test.name", testName);
        _testActivity?.SetTag("test.class", GetType().FullName);
        _testActivity?.SetTag("test.framework", "xUnit");
        _testActivity?.AddEvent(new ActivityEvent("Test started"));
    }

    /// <summary>
    /// Sets a tag on the test activity.
    /// </summary>
    protected void SetTag(string key, object? value)
    {
        _testActivity?.SetTag(key, value);
    }

    /// <summary>
    /// Adds an event to the test activity.
    /// </summary>
    protected void AddEvent(string eventName)
    {
        _testActivity?.AddEvent(new ActivityEvent(eventName));
    }

    /// <summary>
    /// Records evaluation metrics on the test activity.
    /// </summary>
    protected void RecordEvaluationMetrics<T>(IEnumerable<KeyValuePair<string, T>> metrics, Func<T, bool> isFailed)
    {
        if (_testActivity == null) return;

        int index = 0;
        foreach (var metric in metrics)
        {
            bool failed = isFailed(metric.Value);
            bool passed = !failed;

            _testActivity.SetTag($"evaluation.metric.{index}.name", metric.Key);
            _testActivity.SetTag($"evaluation.metric.{index}.passed", passed);
            _testActivity.SetTag($"evaluation.metric.{index}.failed", failed);

            if (failed)
            {
                _testActivity.AddEvent(new ActivityEvent($"Metric '{metric.Key}' failed"));
            }

            index++;
        }

        _testActivity.SetTag("evaluation.metrics.count", index);
    }

    /// <summary>
    /// Completes the main test activity.
    /// </summary>
    protected void CompleteTest(bool success = true, string? message = null)
    {
        if (_testActivity != null)
        {
            _testActivity.SetStatus(success ? ActivityStatusCode.Ok : ActivityStatusCode.Error, message);
            if (success)
            {
                _testActivity.AddEvent(new ActivityEvent("Test passed"));
            }
            else
            {
                _testActivity.AddEvent(new ActivityEvent($"Test failed: {message ?? "Unknown error"}"));
            }
        }
    }

    /// <summary>
    /// Records an exception on the test activity.
    /// </summary>
    protected void RecordException(Exception ex)
    {
        _testActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        _testActivity?.AddException(ex);
        _testActivity?.AddEvent(new ActivityEvent($"Test failed with exception: {ex.Message}"));
    }

    public virtual void Dispose()
    {
        _testActivity?.Dispose();
        _activitySource.Dispose();
    }
}
