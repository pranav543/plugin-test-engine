using TestEngine.Core.Interfaces;
using TestEngine.Core.Models;

namespace TestEngine.Steps;

/// <summary>
/// Checks that a device's current draw is within spec.
///
/// Required parameters:
///   expected  — target current in amps (e.g. "1.5")
///   tolerance — acceptable deviation ± in amps (e.g. "0.05")
///
/// Optional parameters:
///   rail — which power rail (default "main")
/// </summary>
public class CurrentCheckStep : ITestStep
{
    public string StepName    => "CurrentCheck";
    public string Description => $"Current draw check on {Parameters.GetValueOrDefault("rail", "main")} rail";
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
                $"Measured {measured:F4}A — within ±{tolerance}A of {expected}A")
            : StepResult.Fail(StepName, Description, measured, expected,
                $"Measured {measured:F4}A — OUTSIDE ±{tolerance}A of {expected}A"));
    }

    private static double SimulateReading(double expected, double tolerance)
    {
        var rng = new Random();
        double noise = (rng.NextDouble() * 2 - 1) * (tolerance * 0.9);
        return Math.Round(expected + noise, 4);
    }
}