using TestEngine.Console;
using TestEngine.Core.Models;
using TestEngine.Core.Runners;
using TestEngine.Steps;

// ── Banner ────────────────────────────────────────────────────────────────────
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine(@"
  ╔══════════════════════════════════════════════╗
  ║     SEL Plugin-Based Test Step Engine        ║
  ║     Automated Functional Test Runner         ║
  ╚══════════════════════════════════════════════╝
");
Console.ResetColor();

// ── Set up the registry and runner ───────────────────────────────────────────
var registry = new StepRegistry();
var runner   = new ScriptRunner(typeName => registry.Create(typeName));

Console.WriteLine("Registered step types:");
foreach (var name in registry.RegisteredStepNames)
    Console.WriteLine($"  • {name}");
Console.WriteLine();

// ── Collect runtime info from user ───────────────────────────────────────────
Console.Write("Device Serial Number (e.g. SN-001): ");
string deviceId = Console.ReadLine() ?? "SN-UNKNOWN";

Console.Write("Device Model [SEL-300G / SEL-400 / SEL-500]: ");
string deviceModel = Console.ReadLine() ?? "Unknown";

Console.Write("Technician Name: ");
string techName = Console.ReadLine() ?? "Unknown";

// ── Pick a script ─────────────────────────────────────────────────────────────
var scriptDir  = Path.Combine(AppContext.BaseDirectory, "scripts");
var scriptFiles = Directory.GetFiles(scriptDir, "*.json");

Console.WriteLine();
Console.WriteLine("Available test scripts:");
for (int i = 0; i < scriptFiles.Length; i++)
    Console.WriteLine($"  [{i + 1}] {Path.GetFileNameWithoutExtension(scriptFiles[i])}");

Console.Write($"Select script [1-{scriptFiles.Length}]: ");
string choice = Console.ReadLine() ?? "1";
int scriptIndex = int.TryParse(choice, out int idx) && idx >= 1 && idx <= scriptFiles.Length
    ? idx - 1
    : 0;

string chosenScript = scriptFiles[scriptIndex];
Console.WriteLine($"\nRunning: {Path.GetFileName(chosenScript)}");

// ── Build context and run ─────────────────────────────────────────────────────
var context = new TestContext
{
    DeviceId       = deviceId,
    DeviceModel    = deviceModel,
    TechnicianName = techName,
    SimulationMode = true,   // set to false when real hardware is present
};

var report = await runner.RunFromFileAsync(chosenScript, context);

// ── Print report ──────────────────────────────────────────────────────────────
ReportPrinter.Print(report);