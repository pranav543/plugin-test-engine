using System.Diagnostics;
using System.Text.Json;
using TestEngine.Core.Interfaces;
using TestEngine.Core.Models;

namespace TestEngine.Core.Runners;

/// <summary>
/// Orchestrates the execution of a TestScript.
/// The runner itself knows nothing about specific step types —
/// it only works through the ITestStep interface.
/// This means it never needs to change when new steps are added.
/// </summary>
public class ScriptRunner
{
    /// <summary>
    /// Factory function that the runner calls to resolve each step by type name.
    /// Injected as a delegate so the runner stays decoupled from StepRegistry
    /// (and so it's easy to mock in tests).
    /// </summary>
    private readonly Func<string, ITestStep?> _stepFactory;

    public ScriptRunner(Func<string, ITestStep?> stepFactory)
    {
        _stepFactory = stepFactory;
    }

    /// <summary>
    /// Deserializes a JSON file and runs it as a TestScript.
    /// </summary>
    public async Task<ScriptRunReport> RunFromFileAsync(string jsonPath, TestContext context)
    {
        if (!File.Exists(jsonPath))
            throw new FileNotFoundException($"Script not found: {jsonPath}");

        var json    = await File.ReadAllTextAsync(jsonPath);
        var script  = JsonSerializer.Deserialize<TestScript>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Failed to deserialize script.");

        return await RunAsync(script, context);
    }

    /// <summary>
    /// Executes a deserialized TestScript against the given context.
    /// Steps run sequentially; a step failure does NOT abort the run
    /// (all steps always execute so you get a complete picture of the device).
    /// </summary>
    public async Task<ScriptRunReport> RunAsync(TestScript script, TestContext context)
    {
        var results    = new List<StepResult>();
        var runWatch   = Stopwatch.StartNew();

        foreach (var scriptStep in script.Steps)
        {
            // Resolve the step implementation from the registry
            var step = _stepFactory(scriptStep.Type);

            if (step is null)
            {
                // Unknown step type — record a failure and continue
                results.Add(StepResult.Fail(
                    stepName:    scriptStep.Type,
                    description: $"Unknown step type: '{scriptStep.Type}'",
                    message:     $"'{scriptStep.Type}' is not registered in the StepRegistry."));
                continue;
            }

            // Inject parameters from the JSON into the step instance
            step.Parameters = scriptStep.Parameters;

            // Time the individual step
            var stepWatch = Stopwatch.StartNew();
            var result    = await step.ExecuteAsync(context);
            stepWatch.Stop();

            // Capture timing (StepResult is init-only, so we reconstruct with duration)
            results.Add(result with { Duration = stepWatch.Elapsed });
        }

        runWatch.Stop();

        return new ScriptRunReport
        {
            ScriptName    = script.Name,
            TargetModel   = script.TargetModel,
            ScriptVersion = script.Version,
            DeviceId      = context.DeviceId,
            DeviceModel   = context.DeviceModel,
            TechnicianName = context.TechnicianName,
            StartedAt     = DateTime.UtcNow - runWatch.Elapsed,
            TotalDuration = runWatch.Elapsed,
            StepResults   = results,
        };
    }
}

/// <summary>
/// Immutable summary of a completed test run.
/// </summary>
public class ScriptRunReport
{
    public string ScriptName     { get; init; } = string.Empty;
    public string TargetModel    { get; init; } = string.Empty;
    public string ScriptVersion  { get; init; } = string.Empty;
    public string DeviceId       { get; init; } = string.Empty;
    public string DeviceModel    { get; init; } = string.Empty;
    public string TechnicianName { get; init; } = string.Empty;
    public DateTime StartedAt    { get; init; }
    public TimeSpan TotalDuration { get; init; }
    public List<StepResult> StepResults { get; init; } = new();

    public int  TotalSteps  => StepResults.Count;
    public int  PassedSteps => StepResults.Count(r => r.Passed);
    public int  FailedSteps => StepResults.Count(r => !r.Passed);
    public bool OverallPass => StepResults.All(r => r.Passed);
    public double PassRate  => TotalSteps == 0 ? 0
        : Math.Round((double)PassedSteps / TotalSteps * 100, 1);
}