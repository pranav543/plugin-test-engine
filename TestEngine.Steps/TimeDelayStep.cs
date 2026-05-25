using TestEngine.Core.Interfaces;
using TestEngine.Core.Models;

namespace TestEngine.Steps;

/// <summary>
/// Pauses execution for a specified duration.
/// Used between steps that require a device to stabilize before the
/// next measurement (e.g., wait for capacitors to charge after power-on).
///
/// Required parameters:
///   milliseconds — how long to wait (e.g. "500")
///
/// Optional parameters:
///   reason — why we're waiting (shown in the report)
/// </summary>
public class TimeDelayStep : ITestStep
{
    public string StepName    => "TimeDelay";
    public string Description =>
        $"Wait {Parameters.GetValueOrDefault("milliseconds", "0")}ms — " +
        Parameters.GetValueOrDefault("reason", "stabilization delay");
    public Dictionary<string, string> Parameters { get; set; } = new();

    public async Task<StepResult> ExecuteAsync(TestContext context)
    {
        if (!int.TryParse(Parameters.GetValueOrDefault("milliseconds"), out int ms) || ms < 0)
            return StepResult.Fail(StepName, Description, message: "Invalid 'milliseconds' parameter.");

        // In simulation mode, skip the real delay so tests run instantly
        if (!context.SimulationMode)
            await Task.Delay(ms);

        return StepResult.Pass(StepName, Description,
            message: $"Waited {ms}ms successfully.");
    }
}