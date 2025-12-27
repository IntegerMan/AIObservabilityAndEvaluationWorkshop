using System.Diagnostics;

namespace AIObservabilityAndEvaluationWorkshop.Tests;

/// <summary>
/// Base class for tests that provides automatic telemetry instrumentation.
/// Creates a single OpenTelemetry activity per test for observability in Aspire.
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
    /// Starts the main test activity. Called at the beginning of each test.
    /// </summary>
    protected void StartTest(string testName)
    {
        _testActivity = _activitySource.StartActivity($"Test: {testName}", ActivityKind.Internal);
        _testActivity?.SetTag("test.name", testName);
    }

    /// <summary>
    /// Completes the test activity. Sets error status if the test failed.
    /// </summary>
    protected void CompleteTest(bool success = true, string? errorMessage = null)
    {
        if (_testActivity != null)
        {
            _testActivity.SetStatus(
                success ? ActivityStatusCode.Ok : ActivityStatusCode.Error, 
                errorMessage);
        }
    }

    /// <summary>
    /// Records an exception on the test activity.
    /// </summary>
    protected void RecordException(Exception ex)
    {
        _testActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        _testActivity?.AddException(ex);
    }

    public virtual void Dispose()
    {
        _testActivity?.Dispose();
        _activitySource.Dispose();
    }
}
