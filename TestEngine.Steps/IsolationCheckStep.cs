using TestEngine.Core.Interfaces;
using TestEngine.Core.Models;

namespace TestEngine.Steps;

/// <summary>
/// Measures insulation resistance to verify electrical isolation between
/// two conductors. A high resistance reading is GOOD (circuit is isolated).
///
/// Required parameters:
///   minResistance — minimum acceptable isolation resistance in ohms (e.g. "2000")
///   testVoltage   — DC test voltage to apply in volts (e.g. "500")
/// </summary>
public class IsolationCheckStep : ITestStep
{
    public string StepName    => "IsolationCheck";
    public string Description => $"Isolation test at {Parameters.GetValueOrDefault("testVoltage", "500")}V DC";
    public Dictionary<string, string> Parameters { get; set; } = new();

    public Task<StepResult> ExecuteAsync(TestContext context)
    {
        if (!double.TryParse(Parameters.GetValueOrDefault("minResistance"), out double minR))
            return Task.FromResult(StepResult.Fail(StepName, Description,
                message: "Missing or invalid 'minResistance' parameter."));

        double measured = context.SimulationMode
            ? SimulateReading(minR)
            : throw new NotImplementedException("Hardware driver not connected.");

        bool passed = measured >= minR;

        return Task.FromResult(passed
            ? StepResult.Pass(StepName, Description, measured, minR,
                $"Isolation {measured:F0}Ω — adequate (min {minR:F0}Ω)")
            : StepResult.Fail(StepName, Description, measured, minR,
                $"Isolation {measured:F0}Ω — BELOW minimum {minR:F0}Ω — insulation fault"));
    }

    private static double SimulateReading(double minR)
    {
        var rng = new Random();
        bool willPass = rng.NextDouble() > 0.1;
        return willPass
            ? Math.Round(minR + rng.NextDouble() * (minR * 0.5))
            : Math.Round(rng.NextDouble() * (minR * 0.8));
    }
}
