namespace TestEngine.Core.Models;

/// <summary>
/// Shared state passed to every step during a script run.
/// Steps can read device info and store intermediate measurements
/// for later steps to reference (e.g., a CalibrationStep writing
/// an offset that a VoltageCheck then applies).
/// </summary>
public class TestContext
{
    /// <summary>Serial number of the device under test.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Product model name (e.g. "SEL-300G").</summary>
    public string DeviceModel { get; set; } = string.Empty;

    /// <summary>Name of the technician running this script.</summary>
    public string TechnicianName { get; set; } = string.Empty;

    /// <summary>
    /// Arbitrary key-value store that steps can use to share data.
    /// Example: a CalibrationStep writes "offset" = "0.3"; a later
    /// VoltageCheckStep reads it and adjusts its tolerance.
    /// </summary>
    public Dictionary<string, string> SharedData { get; } = new();

    /// <summary>
    /// Whether to simulate hardware readings (true) or attempt
    /// real hardware calls (false). Simulation mode is used for
    /// testing without physical instruments attached.
    /// </summary>
    public bool SimulationMode { get; set; } = true;
}