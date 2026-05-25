namespace TestEngine.Core.Models;

/// <summary>
/// Represents a JSON test script file.
/// The runner deserializes the file into this object, then
/// uses the StepRegistry to instantiate each step by its "type" string.
/// </summary>
public class TestScript
{
    /// <summary>Human-readable name for this test script.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Target device model (informational — shown in report).</summary>
    public string TargetModel { get; set; } = string.Empty;

    /// <summary>Version of this script (for change tracking).</summary>
    public string Version { get; set; } = "1.0";

    /// <summary>Ordered list of steps to execute.</summary>
    public List<ScriptStep> Steps { get; set; } = new();
}

/// <summary>
/// A single step entry in the JSON script.
/// The "type" string is used by StepRegistry to look up the right ITestStep class.
/// </summary>
public class ScriptStep
{
    /// <summary>Must match ITestStep.StepName of the target implementation.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Step-specific configuration. Keys and expected values are defined
    /// by each ITestStep implementation.
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();
}