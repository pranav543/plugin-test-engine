using TestEngine.Core.Interfaces;
using TestEngine.Core.Models;

namespace TestEngine.Steps;

/// <summary>
/// Verifies electrical continuity on a circuit path.
/// A resistance reading below the threshold means the circuit is closed (Pass).
///
/// Required parameters:
///   maxResistance — maximum acceptable resistance in ohms (e.g. "1.0")
///
/// Optional parameters:
///   node — test node label shown in the report (default "J1")
/// </summary>
public class ContinuityCheckStep : ITestStep
{
    public string StepName    => "ContinuityCheck";
    public string Description => $"Continuity check at node {Parameters.GetValueOrDefault("node", "J1")}";
    public Dictionary<string, string> Parameters { get; set; } = new();

    public Task<StepResult> ExecuteAsync(TestContext context)
    {
        if (!double.TryParse(Parameters.GetValueOrDefault("maxResistance"), out double maxR))
            return Task.FromResult(StepResult.Fail(StepName, Description,
                message: "Missing or invalid 'maxResistance' parameter."));

        double measured = context.SimulationMode
            ? SimulateReading(maxR)
            : throw new NotImplementedException("Hardware driver not connected.");

        bool passed = measured <= maxR;

        return Task.FromResult(passed
            ? StepResult.Pass(StepName, Description, measured, maxR,
                $"Resistance {measured:F3}Ω — circuit closed (max {maxR}Ω)")
            : StepResult.Fail(StepName, Description, measured, maxR,
                $"Resistance {measured:F3}Ω — open circuit detected (max {maxR}Ω)"));
    }

    private static double SimulateReading(double maxR)
    {
        var rng = new Random();
        // 85% chance of a good reading well below threshold; 15% chance of failure
        bool willPass = rng.NextDouble() > 0.15;
        return willPass
            ? Math.Round(rng.NextDouble() * (maxR * 0.4), 4)   // well within spec
            : Math.Round(maxR + rng.NextDouble() * 20, 4);      // open circuit
    }
}