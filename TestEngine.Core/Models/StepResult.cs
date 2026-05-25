namespace TestEngine.Core.Models;

/// <summary>
/// The outcome of a single test step execution.
/// Collected by the runner and printed in the final report.
/// </summary>
public record StepResult
{
    public string StepName    { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool   Passed      { get; init; }

    /// <summary>The actual reading or measurement taken during the step.</summary>
    public double? MeasuredValue  { get; init; }

    /// <summary>The target/nominal value the step was checking against.</summary>
    public double? ExpectedValue  { get; init; }

    /// <summary>Pass/fail detail or error description.</summary>
    public string  Message    { get; init; } = string.Empty;

    public DateTime ExecutedAt { get; init; } = DateTime.UtcNow;
    public TimeSpan Duration   { get; init; }

    // ── Factory helpers ───────────────────────────────────────────────────────

    public static StepResult Pass(string stepName, string description,
        double? measured = null, double? expected = null, string message = "")
        => new()
        {
            StepName      = stepName,
            Description   = description,
            Passed        = true,
            MeasuredValue = measured,
            ExpectedValue = expected,
            Message       = message,
        };

    public static StepResult Fail(string stepName, string description,
        double? measured = null, double? expected = null, string message = "")
        => new()
        {
            StepName      = stepName,
            Description   = description,
            Passed        = false,
            MeasuredValue = measured,
            ExpectedValue = expected,
            Message       = message,
        };
}