using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

IHost host = builder.Build();
await host.StartAsync();

// Log that we're starting tests
var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("TestRunner");
logger.LogInformation("Starting unit tests...");

using Activity? programActivity = new ActivitySource("AIObservabilityAndEvaluationWorkshop.Tests").StartActivity("Run Tests", ActivityKind.Internal);
programActivity?.SetTag("test.project", "AIObservabilityAndEvaluationWorkshop.Tests");

try
{
    var testAssembly = typeof(AIObservabilityAndEvaluationWorkshop.Tests.FluencyEvaluatorTest).Assembly;
    var assemblyName = testAssembly.GetName().Name;
    
    logger.LogInformation("Test assembly: {AssemblyName}", assemblyName);
    programActivity?.SetTag("test.assembly", assemblyName ?? "Unknown");

    // Discover test classes and methods using reflection
    var testTypes = testAssembly.GetTypes()
        .Where(t => t.GetCustomAttribute<FactAttribute>() != null || 
                   t.GetMethods().Any(m => m.GetCustomAttribute<FactAttribute>() != null))
        .ToList();

    // If no test classes found, look for classes with [Fact] methods
    if (testTypes.Count == 0)
    {
        testTypes = testAssembly.GetTypes()
            .Where(t => t.GetMethods().Any(m => m.GetCustomAttribute<FactAttribute>() != null))
            .ToList();
    }

    int totalTests = 0;
    int passed = 0;
    int failed = 0;

    foreach (var testType in testTypes)
    {
        var testMethods = testType.GetMethods()
            .Where(m => m.GetCustomAttribute<FactAttribute>() != null)
            .ToList();

        foreach (var testMethod in testMethods)
        {
            totalTests++;
            logger.LogInformation("Running test: {TestClass}.{TestMethod}", testType.Name, testMethod.Name);
            
            using Activity? testActivity = new ActivitySource("AIObservabilityAndEvaluationWorkshop.Tests")
                .StartActivity($"Test: {testType.Name}.{testMethod.Name}", ActivityKind.Internal);
            testActivity?.SetTag("test.class", testType.Name);
            testActivity?.SetTag("test.method", testMethod.Name);

            try
            {
                // Create instance of test class
                var testInstance = Activator.CreateInstance(testType);
                
                // Invoke the test method
                if (testMethod.ReturnType == typeof(Task))
                {
                    var task = (Task)testMethod.Invoke(testInstance, null)!;
                    await task;
                }
                else if (testMethod.ReturnType.IsGenericType && 
                         testMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var task = (Task)testMethod.Invoke(testInstance, null)!;
                    await task;
                }
                else
                {
                    testMethod.Invoke(testInstance, null);
                }

                passed++;
                logger.LogInformation("✓ Test passed: {TestClass}.{TestMethod}", testType.Name, testMethod.Name);
                testActivity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                failed++;
                logger.LogError("✗ Test failed: {TestClass}.{TestMethod} - {ErrorMessage}", 
                    testType.Name, testMethod.Name, ex.InnerException?.Message ?? ex.Message);
                testActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                testActivity?.AddException(ex);
                
                // Re-throw to maintain test failure behavior
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
                throw;
            }
        }
    }

    logger.LogInformation("Test execution completed. Total: {Total}, Passed: {Passed}, Failed: {Failed}", 
        totalTests, passed, failed);
    programActivity?.SetTag("test.total", totalTests);
    programActivity?.SetTag("test.passed", passed);
    programActivity?.SetTag("test.failed", failed);
    programActivity?.SetStatus(failed == 0 ? ActivityStatusCode.Ok : ActivityStatusCode.Error);

    // For console apps, give telemetry time to be exported before exiting
    await Task.Delay(TimeSpan.FromSeconds(2));

    return failed > 0 ? 1 : 0;
}
catch (Exception ex)
{
    logger.LogError(ex, "Error running tests");
    programActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
    programActivity?.AddException(ex);
    
    await Task.Delay(TimeSpan.FromSeconds(2));
    return 1;
}
finally
{
    await host.StopAsync();
}
