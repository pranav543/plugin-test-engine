using TestEngine.Core.Interfaces;

namespace TestEngine.Steps;

/// <summary>
/// Factory that maps step type name strings (from JSON scripts) to
/// concrete ITestStep implementations.
///
/// To add a new step type:
///   1. Create the class (implements ITestStep)
///   2. Add one line here: _registry["YourStepName"] = () => new YourStep();
///   3. Done — no other files change.
///
/// This is the Open/Closed Principle: open for extension, closed for modification.
/// </summary>
public class StepRegistry
{
    // Maps "TypeName" → factory function that creates a fresh instance
    private readonly Dictionary<string, Func<ITestStep>> _registry;

    public StepRegistry()
    {
        _registry = new Dictionary<string, Func<ITestStep>>(StringComparer.OrdinalIgnoreCase)
        {
            ["VoltageCheck"]    = () => new VoltageCheckStep(),
            ["ContinuityCheck"] = () => new ContinuityCheckStep(),
            ["IsolationCheck"]  = () => new IsolationCheckStep(),
            ["CurrentCheck"]    = () => new CurrentCheckStep(),
            ["TimeDelay"]       = () => new TimeDelayStep(),
            ["LogMessage"]      = () => new LogMessageStep(),
        };
    }

    /// <summary>
    /// Returns a fresh instance of the requested step type, or null if not registered.
    /// </summary>
    public ITestStep? Create(string typeName)
    {
        return _registry.TryGetValue(typeName, out var factory)
            ? factory()
            : null;
    }

    /// <summary>
    /// Returns the list of all registered step type names.
    /// Used for validation and tooling.
    /// </summary>
    public IReadOnlyList<string> RegisteredStepNames =>
        _registry.Keys.OrderBy(k => k).ToList();

    /// <summary>
    /// Returns true if the given type name has a registered implementation.
    /// </summary>
    public bool IsRegistered(string typeName) =>
        _registry.ContainsKey(typeName);
}