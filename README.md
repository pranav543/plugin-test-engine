# Plugin-Based Test Step Engine

A C# engine that executes JSON-defined hardware test scripts using a
plugin architecture. Each test step type is an independent class implementing
a shared interface, making the system trivially extensible.

**Stack:** .NET 8 · C# · xUnit · Moq · FluentAssertions

## Architecture

```
JSON Script File
      │  (deserialize)
      ▼
 ScriptRunner                  ← knows only ITestStep, never concrete types
      │  (typeName string)
      ▼
 StepRegistry                  ← factory maps strings → ITestStep instances
      │
      ▼
 ITestStep  (interface)
   ├── VoltageCheckStep         implemented steps (the "plugins")
   ├── ContinuityCheckStep
   ├── IsolationCheckStep
   ├── CurrentCheckStep
   ├── TimeDelayStep
   ├── LogMessageStep
   └── FrequencyCheckStep      ← added with zero changes to runner/registry interface
      │
      ▼
 ScriptRunReport               ← immutable results collected and printed
```

## Running the App

```bash
cd TestEngine.Console
dotnet run
```

You'll be prompted for a device serial number, model, technician name,
and which test script to run. Results print with color-coded PASS/FAIL.

---

## Running Tests

```bash
dotnet test --verbosity normal
```

---
