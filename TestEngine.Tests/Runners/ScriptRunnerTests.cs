using FluentAssertions;
using Moq;
using TestEngine.Core.Interfaces;
using TestEngine.Core.Models;
using TestEngine.Core.Runners;

namespace TestEngine.Tests.Runners;

public class ScriptRunnerTests
{
    private static TestContext SimContext => new()
    {
        DeviceId       = "SN-TEST",
        DeviceModel    = "SEL-300G",
        TechnicianName = "TestBot",
        SimulationMode = true,
    };

    // ── Helper: build a mock step that always passes ──────────────────────────
    private static ITestStep MakePassStep(string name = "MockStep")
    {
        var mock = new Mock<ITestStep>();
        mock.Setup(s => s.StepName).Returns(name);
        mock.Setup(s => s.Description).Returns($"{name} description");
        mock.SetupProperty(s => s.Parameters, new Dictionary<string, string>());
        mock.Setup(s => s.ExecuteAsync(It.IsAny<TestContext>()))
            .ReturnsAsync(StepResult.Pass(name, $"{name} description", message: "ok"));
        return mock.Object;
    }

    private static ITestStep MakeFailStep(string name = "MockFailStep")
    {
        var mock = new Mock<ITestStep>();
        mock.Setup(s => s.StepName).Returns(name);
        mock.Setup(s => s.Description).Returns($"{name} description");
        mock.SetupProperty(s => s.Parameters, new Dictionary<string, string>());
        mock.Setup(s => s.ExecuteAsync(It.IsAny<TestContext>()))
            .ReturnsAsync(StepResult.Fail(name, $"{name} description", message: "fail"));
        return mock.Object;
    }

    // ── RunAsync basic tests ──────────────────────────────────────────────────
    [Fact]
    public async Task RunAsync_ReturnsReport_WithCorrectStepCount()
    {
        var passStep = MakePassStep();
        var runner   = new ScriptRunner(_ => passStep);
        var script   = new TestScript
        {
            Name  = "Test",
            Steps = new()
            {
                new ScriptStep { Type = "MockStep" },
                new ScriptStep { Type = "MockStep" },
                new ScriptStep { Type = "MockStep" },
            }
        };

        var report = await runner.RunAsync(script, SimContext);

        report.TotalSteps.Should().Be(3);
    }

    [Fact]
    public async Task RunAsync_OverallPass_WhenAllStepsPass()
    {
        var passStep = MakePassStep();
        var runner   = new ScriptRunner(_ => passStep);
        var script   = new TestScript
        {
            Steps = new() { new ScriptStep { Type = "A" }, new ScriptStep { Type = "B" } }
        };

        var report = await runner.RunAsync(script, SimContext);

        report.OverallPass.Should().BeTrue();
        report.FailedSteps.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_OverallFail_WhenAnyStepFails()
    {
        int call = 0;
        // First step passes, second fails
        var runner = new ScriptRunner(_ => call++ == 0 ? MakePassStep() : MakeFailStep());
        var script = new TestScript
        {
            Steps = new() { new ScriptStep { Type = "A" }, new ScriptStep { Type = "B" } }
        };

        var report = await runner.RunAsync(script, SimContext);

        report.OverallPass.Should().BeFalse();
        report.FailedSteps.Should().Be(1);
        report.PassedSteps.Should().Be(1);
    }

    // ── Unknown step type ─────────────────────────────────────────────────────
    [Fact]
    public async Task RunAsync_RecordsFailureResult_ForUnknownStepType()
    {
        // Factory returns null = step type not registered
        var runner = new ScriptRunner(_ => null);
        var script = new TestScript
        {
            Steps = new() { new ScriptStep { Type = "FrequencyCheck" } }
        };

        var report = await runner.RunAsync(script, SimContext);

        report.TotalSteps.Should().Be(1);
        report.OverallPass.Should().BeFalse();
        report.StepResults[0].Message.Should().Contain("not registered");
    }

    // ── All steps execute even after a failure ────────────────────────────────
    [Fact]
    public async Task RunAsync_ExecutesAllSteps_EvenAfterOneStepFails()
    {
        int executionCount = 0;
        var runner = new ScriptRunner(typeName =>
        {
            var mock = new Mock<ITestStep>();
            mock.Setup(s => s.StepName).Returns("S");
            mock.Setup(s => s.Description).Returns("desc");
            mock.SetupProperty(s => s.Parameters, new Dictionary<string, string>());
            mock.Setup(s => s.ExecuteAsync(It.IsAny<TestContext>()))
                .ReturnsAsync(() =>
                {
                    executionCount++;
                    // Always fail
                    return StepResult.Fail("S", "desc");
                });
            return mock.Object;
        });

        var script = new TestScript
        {
            Steps = new()
            {
                new ScriptStep { Type = "S" },
                new ScriptStep { Type = "S" },
                new ScriptStep { Type = "S" },
            }
        };

        await runner.RunAsync(script, SimContext);

        executionCount.Should().Be(3,
            "all steps must run regardless of earlier failures");
    }

    // ── Report metadata ───────────────────────────────────────────────────────
    [Fact]
    public async Task RunAsync_Report_ContainsCorrectDeviceAndTechnicianInfo()
    {
        var passStep = MakePassStep();
        var runner   = new ScriptRunner(_ => passStep);
        var context  = new TestContext
        {
            DeviceId       = "SN-XYZ",
            DeviceModel    = "SEL-500",
            TechnicianName = "Alice",
            SimulationMode = true,
        };
        var script = new TestScript { Name = "Smoke", Steps = new() { new ScriptStep { Type = "X" } } };

        var report = await runner.RunAsync(script, context);

        report.DeviceId.Should().Be("SN-XYZ");
        report.DeviceModel.Should().Be("SEL-500");
        report.TechnicianName.Should().Be("Alice");
    }

    // ── PassRate calculation ──────────────────────────────────────────────────
    [Fact]
    public async Task RunAsync_EmptyScript_ProducesZeroPassRate()
    {
        var runner = new ScriptRunner(_ => MakePassStep());
        var script = new TestScript { Steps = new() };

        var report = await runner.RunAsync(script, SimContext);

        report.TotalSteps.Should().Be(0);
        report.PassRate.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_PassRate_IsCalculatedCorrectly()
    {
        int call = 0;
        // 3 pass, 1 fail = 75%
        var runner = new ScriptRunner(_ => call++ < 3 ? MakePassStep() : MakeFailStep());
        var script = new TestScript
        {
            Steps = Enumerable.Range(0, 4)
                              .Select(_ => new ScriptStep { Type = "X" })
                              .ToList()
        };

        var report = await runner.RunAsync(script, SimContext);

        report.PassRate.Should().Be(75.0);
    }
}