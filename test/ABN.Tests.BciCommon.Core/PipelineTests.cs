using ABN.BciCommon;
using ABN.BciCommon.Stages;
using ABN.BciCommon.Stages.Base;
using ABN.Tests.BciCommon.TestSupport;

using Microsoft.Extensions.Logging.Abstractions;

namespace ABN.Tests.BciCommon.Core;

public class PipelineTests : IDisposable
{
    private readonly Pipeline _pipeline;

    public PipelineTests()
    {
        var lf = new NullLoggerFactory();
        var logger = lf.CreateLogger("TestLogger");

        _pipeline = new Pipeline(logger);
    }

    public void Dispose() { }

    [Fact]
    public async Task PipelineWithSingleStageAsync()
    {
        var stage = new GuidStage(_pipeline);

        await _pipeline.ExecuteAsync();

        Assert.Equal(StageState.CompletedSuccess, stage.State);
    }

    [Fact]
    public async Task PipelineWithTwoStagesAsync()
    {
        var stage1 = new GuidStage(_pipeline);
        var stage2 = stage1.AddChildStage(new GuidStage(_pipeline));

        await _pipeline.ExecuteAsync();

        Assert.Equal(StageState.CompletedSuccess, stage1.State);
        Assert.Equal(StageState.CompletedSuccess, stage2.State);
    }

    [Fact]
    public async Task PipelineDoesNotRunChildStagesWithFailedParentAsync()
    {
        var stage1 = new FailureStage(_pipeline);
        var stage2 = stage1.AddChildStage(new GuidStage(_pipeline));

        await _pipeline.ExecuteAsync();

        Assert.Equal(StageState.CompletedFailed, stage1.State);
        Assert.Equal(StageState.Skipped, stage2.State);
    }

    [Fact]
    public async Task PipelineWithStageWithMultipleDependenciesRunsAllAsync()
    {
        var stage1 = new GuidStage(_pipeline);
        var stage2 = stage1.AddChildStage(new GuidStage(_pipeline));
        var stage3 = stage1.AddChildStage(new GuidStage(_pipeline));

        await _pipeline.ExecuteAsync();

        Assert.Equal(StageState.CompletedSuccess, stage1.State);
        Assert.Equal(StageState.CompletedSuccess, stage2.State);
        Assert.Equal(StageState.CompletedSuccess, stage3.State);
    }

    [Fact]
    public async Task PipelineWithFailedStageWithMultipleDependenciesRunsNoneAsync()
    {
        var stage1 = new FailureStage(_pipeline);
        var stage2 = stage1.AddChildStage(new GuidStage(_pipeline));
        var stage3 = stage1.AddChildStage(new GuidStage(_pipeline));

        await _pipeline.ExecuteAsync();

        Assert.Equal(StageState.CompletedFailed, stage1.State);
        Assert.Equal(StageState.Skipped, stage2.State);
        Assert.Equal(StageState.Skipped, stage3.State);
    }

    [Fact]
    public async Task PipelineWithFailedStageDoesNotAffectUnconnectedStageAsync()
    {
        var stage1 = new FailureStage(_pipeline);
        var stage2 = new GuidStage(_pipeline);
        var stage3 = stage1.AddChildStage(new GuidStage(_pipeline));
        var stage4 = stage2.AddChildStage(new GuidStage(_pipeline));

        await _pipeline.ExecuteAsync();

        Assert.Equal(StageState.CompletedFailed, stage1.State);
        Assert.Equal(StageState.CompletedSuccess, stage2.State);
        Assert.Equal(StageState.Skipped, stage3.State);
        Assert.Equal(StageState.CompletedSuccess, stage4.State);
    }

    [Fact]
    public async Task PipelineWithChainOfStagesFailsAllWhenOneFails()
    {
        var stage1 = new GuidStage(_pipeline);
        var stage2 = stage1.AddChildStage(new FailureStage(_pipeline));
        var stage3 = stage2.AddChildStage(new GuidStage(_pipeline));
        var stage4 = stage3.AddChildStage(new GuidStage(_pipeline));

        await _pipeline.ExecuteAsync();

        Assert.Equal(StageState.CompletedSuccess, stage1.State);
        Assert.Equal(StageState.CompletedFailed, stage2.State);
        Assert.Equal(StageState.Skipped, stage3.State);
        Assert.Equal(StageState.Skipped, stage4.State);
    }

    [Fact]
    public async Task PipelineEnforcesTimeout()
    {
        var stage1 = new TimeoutStage(_pipeline);

        await _pipeline.ExecuteAsync();

        Assert.Equal(StageState.TimedOut, stage1.State);
    }

    [Fact]
    public async Task PipelineEnforcesTimeoutAndDoesNotRunChildStages()
    {
        var stage1 = new TimeoutStage(_pipeline);
        var stage2 = stage1.AddChildStage(new GuidStage(_pipeline));

        await _pipeline.ExecuteAsync();

        Assert.Equal(StageState.TimedOut, stage1.State);
        Assert.Equal(StageState.Skipped, stage2.State);
    }
}