using TestEngine.Core.Models;
using TestEngine.Core.Runners;

namespace TestEngine.Console;

/// <summary>
/// Prints a ScriptRunReport to the console with ANSI color formatting.
/// Separated from Program.cs so it can be independently tested.
/// </summary>
public static class ReportPrinter
{
    public static void Print(ScriptRunReport report)
    {
        string sep = new string('─', 65);

        System.Console.WriteLine();
        System.Console.WriteLine(sep);
        System.Console.WriteLine($"  TEST REPORT — {report.ScriptName} v{report.ScriptVersion}");
        System.Console.WriteLine(sep);
        System.Console.WriteLine($"  Device:      {report.DeviceId} ({report.DeviceModel})");
        System.Console.WriteLine($"  Technician:  {report.TechnicianName}");
        System.Console.WriteLine($"  Started:     {report.StartedAt:yyyy-MM-dd HH:mm:ss} UTC");
        System.Console.WriteLine($"  Duration:    {report.TotalDuration.TotalSeconds:F2}s");
        System.Console.WriteLine(sep);
        System.Console.WriteLine();

        foreach (var result in report.StepResults)
        {
            string status  = result.Passed ? "  PASS  " : "  FAIL  ";
            string bracket = result.Passed ? "[ PASS ]" : "[ FAIL ]";

            System.Console.ForegroundColor = result.Passed
                ? ConsoleColor.Green
                : ConsoleColor.Red;
            System.Console.Write($"  {bracket}  ");
            System.Console.ResetColor();

            System.Console.Write($"{result.StepName,-20}");

            if (result.MeasuredValue.HasValue)
                System.Console.Write($"  measured={result.MeasuredValue:F4}");
            if (result.ExpectedValue.HasValue)
                System.Console.Write($"  expected={result.ExpectedValue:F4}");

            System.Console.WriteLine();

            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                System.Console.ForegroundColor = ConsoleColor.DarkGray;
                System.Console.WriteLine($"              └─ {result.Message}");
                System.Console.ResetColor();
            }
        }

        System.Console.WriteLine();
        System.Console.WriteLine(sep);

        // Overall result line
        System.Console.ForegroundColor = report.OverallPass ? ConsoleColor.Green : ConsoleColor.Red;
        string overall = report.OverallPass ? "✓  OVERALL: PASS" : "✗  OVERALL: FAIL";
        System.Console.WriteLine($"  {overall}   ({report.PassedSteps}/{report.TotalSteps} steps passed, {report.PassRate}% pass rate)");
        System.Console.ResetColor();

        System.Console.WriteLine(sep);
        System.Console.WriteLine();
    }
}