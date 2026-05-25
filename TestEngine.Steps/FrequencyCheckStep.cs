using TestEngine.Core.Interfaces;
using TestEngine.Core.Models;

namespace TestEngine.Steps;

/// <summary>
/// Checks that a signal's frequency is within specification.
/// Demonstrates how the plugin system is extended with zero changes
/// to the runner, registry interface, or any existing step.
///
/// Required parameters:
///   expected  — target frequency in Hz (e.g. "60.0")
///   tolerance — acceptable deviation ± in Hz (e.g. "0.5")
///
/// Optional parameters:
///   channel — signal channel name (default "CH1")
/// </summary>
public class FrequencyCheckStep : ITestStep
{
    public string StepName    => "FrequencyCheck";
    public string Description =>
        $"Frequency check on {Parameters.GetValueOrDefault("channel", "CH1")}";
    public Dictionary<string, string> Parameters { get; set; } = new();

    public Task<StepResult> ExecuteAsync(TestContext context)
    {
        if (!double.TryParse(Parameters.GetValueOrDefault("expected"),  out double expected))
            return Task.FromResult(StepResult.Fail(StepName, Description,
                message: "Missing or invalid 'expected' parameter."));

        if (!double.TryParse(Parameters.GetValueOrDefault("tolerance"), out double tolerance))
            return Task.FromResult(StepResult.Fail(StepName, Description,
                message: "Missing or invalid 'tolerance' parameter."));

        double measured = context.SimulationMode
            ? SimulateReading(expected, tolerance)
            : throw new NotImplementedException("Hardware driver not connected.");

        bool passed = Math.Abs(measured - expected) <= tolerance;

        return Task.FromResult(passed
            ? StepResult.Pass(StepName, Description, measured, expected,
                $"Measured {measured:F3}Hz — within ±{tolerance}Hz of {expected}Hz")
            : StepResult.Fail(StepName, Description, measured, expected,
                $"Measured {measured:F3}Hz — OUTSIDE ±{tolerance}Hz of {expected}Hz"));
    }

    private static double SimulateReading(double expected, double tolerance)
    {
        var rng = new Random();
        double noise = (rng.NextDouble() * 2 - 1) * (tolerance * 0.7);
        return Math.Round(expected + noise, 4);
    }
}