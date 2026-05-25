# Plugin-Based Test Step Engine

A C# engine that executes JSON-defined hardware test scripts using a
plugin architecture. Each test step type is an independent class implementing
a shared interface, making the system trivially extensible.

**Stack:** .NET 8 · C# · xUnit · Moq · FluentAssertions