using TestEngine.Core.Models;

namespace TestEngine.Core.Interfaces;

/// <summary>
/// The contract every test step plugin must implement.
///
/// Design intent:
///   - Execute() is the only method consumers call — they don't need to know
///     what kind of step it is.
///   - StepName must match the "type" field in the JSON script so the registry
///     can look up the right class dynamically.
///   - Parameters is a dictionary so each step can define its own config keys
///     without changing the interface.
///
/// Adding a new step type = create one new class, register it. Zero other changes.
/// </summary>
public interface ITestStep
{
    /// <summary>
    /// The unique string identifier for this step type (e.g. "VoltageCheck").
    /// Must match the "type" field in the JSON script.
    /// </summary>
    string StepName { get; }

    /// <summary>
    /// Human-readable description of what this step does.
    /// Shown in the test report.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Key-value configuration loaded from the JSON script.
    /// Each step type defines which keys it expects.
    /// </summary>
    Dictionary<string, string> Parameters { get; set; }

    /// <summary>
    /// Execute the test step and return a result.
    /// The context provides shared state (e.g., device info, prior readings)
    /// that steps can read from and write to.
    /// </summary>
    Task<StepResult> ExecuteAsync(TestContext context);
}