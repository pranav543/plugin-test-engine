using FluentAssertions;
using TestEngine.Core.Models;
using TestEngine.Steps;

namespace TestEngine.Tests.Steps;

public class CurrentCheckStepTests
{
    private static TestContext SimContext => new() { SimulationMode = true };

    [Fact]
    public void StepName_ShouldBe_CurrentCheck()
    {
        new CurrentCheckStep().StepName.Should().Be("CurrentCheck");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFail_WhenExpectedMissing()
    {
        var step = new CurrentCheckStep { Parameters = new() { ["tolerance"] = "0.05" } };

        var result = await step.ExecuteAsync(SimContext);

        result.Passed.Should().BeFalse();
        result.Message.Should().Contain("expected");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFail_WhenToleranceMissing()
    {
        var step = new CurrentCheckStep { Parameters = new() { ["expected"] = "1.5" } };

        var result = await step.ExecuteAsync(SimContext);

        result.Passed.Should().BeFalse();
        result.Message.Should().Contain("tolerance");
    }

    [Fact]
    public async Task ExecuteAsync_PassedIsConsistentWithMeasurement()
    {
        var step = new CurrentCheckStep
        {
            Parameters = new() { ["expected"] = "1.5", ["tolerance"] = "0.05" }
        };

        for (int i = 0; i < 20; i++)
        {
            var result = await step.ExecuteAsync(SimContext);
            double expected   = result.ExpectedValue!.Value;
            double measured   = result.MeasuredValue!.Value;
            bool   shouldPass = Math.Abs(measured - expected) <= 0.05;
            result.Passed.Should().Be(shouldPass);
        }
    }
}