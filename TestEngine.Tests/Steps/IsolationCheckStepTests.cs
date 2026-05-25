using FluentAssertions;
using TestEngine.Core.Models;
using TestEngine.Steps;

namespace TestEngine.Tests.Steps;

public class IsolationCheckStepTests
{
    private static TestContext SimContext => new() { SimulationMode = true };

    [Fact]
    public void StepName_ShouldBe_IsolationCheck()
    {
        new IsolationCheckStep().StepName.Should().Be("IsolationCheck");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFail_WhenMinResistanceMissing()
    {
        var step = new IsolationCheckStep { Parameters = new() };

        var result = await step.ExecuteAsync(SimContext);

        result.Passed.Should().BeFalse();
        result.Message.Should().Contain("minResistance");
    }

    [Fact]
    public async Task ExecuteAsync_PassedIsTrue_WhenMeasuredExceedsMin()
    {
        var step = new IsolationCheckStep
        {
            Parameters = new() { ["minResistance"] = "2000", ["testVoltage"] = "500" }
        };

        // Verify pass/fail is consistent with measured vs expected across runs
        for (int i = 0; i < 20; i++)
        {
            var result = await step.ExecuteAsync(SimContext);
            bool consistent = result.Passed == (result.MeasuredValue >= result.ExpectedValue);
            consistent.Should().BeTrue();
        }
    }

    [Fact]
    public async Task ExecuteAsync_IncludesTestVoltage_InDescription()
    {
        var step = new IsolationCheckStep
        {
            Parameters = new() { ["minResistance"] = "2000", ["testVoltage"] = "1000" }
        };

        // Description should mention the test voltage
        step.Description.Should().Contain("1000");
    }
}