using FluentAssertions;
using TestEngine.Steps;

namespace TestEngine.Tests.Core;

public class StepRegistryTests
{
    private readonly StepRegistry _registry = new();

    // ── Known types ───────────────────────────────────────────────────────────
    [Theory]
    [InlineData("VoltageCheck")]
    [InlineData("ContinuityCheck")]
    [InlineData("IsolationCheck")]
    [InlineData("CurrentCheck")]
    [InlineData("TimeDelay")]
    [InlineData("LogMessage")]
    public void Create_ReturnsNonNull_ForAllRegisteredTypes(string typeName)
    {
        var step = _registry.Create(typeName);

        step.Should().NotBeNull();
    }

    [Theory]
    [InlineData("VoltageCheck")]
    [InlineData("ContinuityCheck")]
    [InlineData("IsolationCheck")]
    [InlineData("CurrentCheck")]
    [InlineData("TimeDelay")]
    [InlineData("LogMessage")]
    public void Create_ReturnsStepWithCorrectStepName(string typeName)
    {
        var step = _registry.Create(typeName);

        step!.StepName.Should().Be(typeName);
    }

    // ── Case insensitivity ────────────────────────────────────────────────────
    [Theory]
    [InlineData("voltagecheck")]
    [InlineData("VOLTAGECHECK")]
    [InlineData("Voltagecheck")]
    public void Create_IsCaseInsensitive(string typeName)
    {
        var step = _registry.Create(typeName);

        step.Should().NotBeNull();
    }

    // ── Unknown type ──────────────────────────────────────────────────────────
    [Fact]
    public void Create_ReturnsNull_ForUnknownType()
    {
        var step = _registry.Create("FrequencyCheck");

        step.Should().BeNull();
    }

    [Fact]
    public void Create_ReturnsNull_ForEmptyString()
    {
        var step = _registry.Create(string.Empty);

        step.Should().BeNull();
    }

    // ── Fresh instances ───────────────────────────────────────────────────────
    [Fact]
    public void Create_ReturnsFreshInstance_EachCall()
    {
        var step1 = _registry.Create("VoltageCheck");
        var step2 = _registry.Create("VoltageCheck");

        // Must be different instances so one script's parameters
        // don't bleed into the next call
        step1.Should().NotBeSameAs(step2);
    }

    // ── IsRegistered helper ───────────────────────────────────────────────────
    [Fact]
    public void IsRegistered_ReturnsTrue_ForKnownType()
    {
        _registry.IsRegistered("VoltageCheck").Should().BeTrue();
    }

    [Fact]
    public void IsRegistered_ReturnsFalse_ForUnknownType()
    {
        _registry.IsRegistered("FrequencyCheck").Should().BeFalse();
    }

    // ── RegisteredStepNames ───────────────────────────────────────────────────
    [Fact]
    public void RegisteredStepNames_ContainsAllSixBuiltInTypes()
    {
        var names = _registry.RegisteredStepNames;

        names.Should().Contain("VoltageCheck")
             .And.Contain("ContinuityCheck")
             .And.Contain("IsolationCheck")
             .And.Contain("CurrentCheck")
             .And.Contain("TimeDelay")
             .And.Contain("LogMessage");
             
    }

    [Fact]
    public void RegisteredStepNames_AreReturnedInAlphabeticalOrder()
    {
        var names = _registry.RegisteredStepNames.ToList();

        names.Should().BeInAscendingOrder();
    }
}