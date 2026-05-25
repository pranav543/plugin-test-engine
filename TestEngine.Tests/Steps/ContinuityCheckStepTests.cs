using FluentAssertions;
using TestEngine.Core.Models;
using TestEngine.Steps;

namespace TestEngine.Tests.Steps;

public class ContinuityCheckStepTests
{
    private static TestContext SimContext => new() { SimulationMode = true };

    [Fact]
    public void StepName_ShouldBe_ContinuityCheck()
    {
        new ContinuityCheckStep().StepName.Should().Be("ContinuityCheck");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFail_WhenMaxResistanceMissing()
    {
        var step = new ContinuityCheckStep { Parameters = new() };

        var result = await step.ExecuteAsync(SimContext);

        result.Passed.Should().BeFalse();
        result.Message.Should().Contain("maxResistance");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFail_WhenMaxResistanceIsNotNumeric()
    {
        var step = new ContinuityCheckStep
        {
            Parameters = new() { ["maxResistance"] = "not-a-number" }
        };

        var result = await step.ExecuteAsync(SimContext);

        result.Passed.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_AlwaysSetsExpectedValue_FromParameter()
    {
        var step = new ContinuityCheckStep
        {
            Parameters = new() { ["maxResistance"] = "1.0" }
        };

        var result = await step.ExecuteAsync(SimContext);

        result.ExpectedValue.Should().Be(1.0);
    }

    [Fact]
    public async Task ExecuteAsync_AlwaysPopulates_MeasuredValue()
    {
        var step = new ContinuityCheckStep
        {
            Parameters = new() { ["maxResistance"] = "1.0" }
        };

        var result = await step.ExecuteAsync(SimContext);

        result.MeasuredValue.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_PassedIsTrue_WhenMeasuredBelowMax()
    {
        // Run many times; given 85% pass rate in simulation, statistically
        // at least some will pass — we test the logic by checking consistency
        var step = new ContinuityCheckStep
        {
            Parameters = new() { ["maxResistance"] = "1.0" }
        };

        // Run 20 times and verify every result is consistent (pass ↔ measured ≤ max)
        for (int i = 0; i < 20; i++)
        {
            var result = await step.ExecuteAsync(SimContext);
            bool consistent = result.Passed == (result.MeasuredValue <= result.ExpectedValue);
            consistent.Should().BeTrue(
                $"Passed={result.Passed} should match MeasuredValue={result.MeasuredValue} ≤ ExpectedValue={result.ExpectedValue}");
        }
    }
}