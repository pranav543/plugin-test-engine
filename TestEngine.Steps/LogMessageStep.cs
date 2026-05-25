using TestEngine.Core.Interfaces;
using TestEngine.Core.Models;

namespace TestEngine.Steps;

/// <summary>
/// Emits a message to the test log / report with no pass/fail check.
/// Useful for annotating reports with section headers, technician prompts,
/// or intermediate state from SharedData.
///
/// Required parameters:
///   message — the text to log
///
/// Optional parameters:
///   level — "info" (default) | "warning" | "section"
/// </summary>
public class LogMessageStep : ITestStep
{
    public string StepName    => "LogMessage";
    public string Description => Parameters.GetValueOrDefault("message", "(no message)");
    public Dictionary<string, string> Parameters { get; set; } = new();

    public Task<StepResult> ExecuteAsync(TestContext context)
    {
        string message = Parameters.GetValueOrDefault("message", "(no message)");
        string level   = Parameters.GetValueOrDefault("level", "info");

        // Optionally interpolate SharedData values into the message
        foreach (var (key, value) in context.SharedData)
            message = message.Replace($"{{{key}}}", value);

        return Task.FromResult(StepResult.Pass(StepName,
            description: $"[{level.ToUpper()}] {message}",
            message: message));
    }
}