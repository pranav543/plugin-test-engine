using TestEngine.Core.Interfaces;
using TestEngine.Core.Models;

namespace TestEngine.Steps;

/// <summary>
/// Checks that a device's output voltage falls within acceptable tolerance.
///
/// Required parameters:
///   expected  — target voltage in volts (e.g. "24.0")
///   tolerance — acceptable deviation ± in volts (e.g. "0.5")
///
/// Optional parameters:
///   channel   — which voltage rail to test (default "primary")
/// </summary>
public class VoltageCheckStep : ITestStep
{
    public string StepName    => "VoltageCheck";
    public string Description => $"Voltage check on {Parameters.GetValueOrDefault("channel", "primary")} rail";
    public Dictionary<string, string> Parameters { get; set; } = new();

    public async Task<StepResult> ExecuteAsync(TestContext context)
    {
        // Parse config from the JSON script
        if (!double.TryParse(Parameters.GetValueOrDefault("expected"),  out double expected))
            return StepResult.Fail(StepName, Description, message: "Missing or invalid 'expected' parameter.");

        if (!double.TryParse(Parameters.GetValueOrDefault("tolerance"), out double tolerance))
            return StepResult.Fail(StepName, Description, message: "Missing or invalid 'tolerance' parameter.");

        // In simulation mode we generate a realistic reading.
        // In production this would call a real instrument driver.
        double measured = context.SimulationMode
            ? SimulateReading(expected, tolerance)
            : await ReadFromHardwareAsync();

        bool passed = Math.Abs(measured - expected) <= tolerance;

        return passed
            ? StepResult.Pass(StepName, Description, measured, expected,
                $"Measured {measured:F3}V — within ±{tolerance}V of {expected}V")
            : StepResult.Fail(StepName, Description, measured, expected,
                $"Measured {measured:F3}V — OUTSIDE ±{tolerance}V of {expected}V");
    }

    private static double SimulateReading(double expected, double tolerance)
    {
        // Simulate a reading centered on expected with small random noise
        var rng    = new Random();
        double noise = (rng.NextDouble() * 2 - 1) * (tolerance * 0.8);
        return Math.Round(expected + noise, 4);
    }

    // Stub: in a real system this would open a VISA/SCPI connection
    private static Task<double> ReadFromHardwareAsync() =>
        throw new NotImplementedException("Hardware driver not connected.");
}