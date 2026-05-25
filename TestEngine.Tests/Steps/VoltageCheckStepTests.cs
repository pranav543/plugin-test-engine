using FluentAssertions;
using TestEngine.Core.Models;
using TestEngine.Steps;

namespace TestEngine.Tests.Steps;

public class VoltageCheckStepTests
{
    private static TestContext SimContext => new() { SimulationMode = true };

    // ── Configuration validation ──────────────────────────────────────────────
    [Fact]
    public async Task ExecuteAsync_ReturnsFail_WhenExpectedParameterMissing()
    {
        var step = new VoltageCheckStep { Parameters = new() { ["tolerance"] = "0.5" } };

        var result = await step.ExecuteAsync(SimContext);

        result.Passed.Should().BeFalse();
        result.Message.Should().Contain("expected");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFail_WhenToleranceParameterMissing()
    {
        var step = new VoltageCheckStep { Parameters = new() { ["expected"] = "24.0" } };

        var result = await step.ExecuteAsync(SimContext);

        result.Passed.Should().BeFalse();
        result.Message.Should().Contain("tolerance");
    }

    // ── Metadata checks ───────────────────────────────────────────────────────
    [Fact]
    public void StepName_ShouldBe_VoltageCheck()
    {
        var step = new VoltageCheckStep();
        step.StepName.Should().Be("VoltageCheck");
    }

    // ── Result structure ──────────────────────────────────────────────────────
    [Fact]
    public async Task ExecuteAsync_AlwaysPopulates_StepName_And_Description()
    {
        var step = new VoltageCheckStep
        {
            Parameters = new() { ["expected"] = "24.0", ["tolerance"] = "0.5" }
        };

        var result = await step.ExecuteAsync(SimContext);

        result.StepName.Should().Be("VoltageCheck");
        result.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_AlwaysPopulates_MeasuredValue()
    {
        var step = new VoltageCheckStep
        {
            Parameters = new() { ["expected"] = "24.0", ["tolerance"] = "0.5" }
        };

        var result = await step.ExecuteAsync(SimContext);

        result.MeasuredValue.Should().NotBeNull();
    }

    // ── Boundary logic ────────────────────────────────────────────────────────
    [Theory]
    [InlineData("24.0", "0.5")]   // typical 24V rail
    [InlineData("12.0", "0.3")]   // 12V rail
    [InlineData("5.0",  "0.15")]  // 5V logic rail
    public async Task ExecuteAsync_ProducesResultWithExpectedValue_MatchingParam(
        string expected, string tolerance)
    {
        var step = new VoltageCheckStep
        {
            Parameters = new() { ["expected"] = expected, ["tolerance"] = tolerance }
        };

        var result = await step.ExecuteAsync(SimContext);

        result.ExpectedValue.Should().BeApproximately(double.Parse(expected), 0.001);
    }
}