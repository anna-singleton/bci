using Microsoft.Extensions.Logging;

namespace ABN.BciCommon.Stages.Base;

public abstract class AbstractStage
{
    public StageState State { get; set; } = StageState.NotStarted;

    protected HashSet<string> ChildStageIds { get; } = [];

    protected HashSet<string> ParentStageIds { get; } = [];

    public string StageId { get; } = Guid.NewGuid().ToString();

    public string StageName { get; }

    protected ILogger Logger => ParentPipeline.Logger;

    private Pipeline ParentPipeline { get; }

    private Task? TimeoutTask { get; }

    protected virtual ValueTask<bool> ShouldExecuteAsync(PipelineContext context)
    {
        return ValueTask.FromResult(true);
    }

    public async Task ExecuteAsync(PipelineContext context)
    {
        using var logscope = Logger.BeginScope($"{StageName}.{StageId}");

        Logger.LogInformation("Determing Whether Stage Should Execute.");

        if (!await ShouldExecuteAsync(context))
        {
            Logger.LogInformation("Stage Skipped.");
            State = StageState.Skipped;
            return;
        }

        Logger.LogInformation("Stage Executing...");

        State = StageState.InProgress;
        var result = await ProcessAsync(context);

        if (result == StageResult.Succeeded)
        {
            Logger.LogInformation("Stage Execution Successfully.");
            State = StageState.CompletedSuccess;
            return;
        }

        Logger.LogWarning("Stage Execution Failed.");
        State = StageState.CompletedFailed;
        return;
    }

    public bool IsComplete => State.IsComplete();
    public bool IsFinished => State.IsCompleteOrSkipped();
    public bool IsPending => State == StageState.NotStarted;
    public bool IsActive => State == StageState.InProgress;
    public bool IsSkipped => State == StageState.Skipped;
    public bool IsSuccessful => State == StageState.CompletedSuccess;
    public bool IsFailed => State == StageState.CompletedFailed;
    public bool IsTimedOut => State == StageState.TimedOut || (TimeoutTask?.IsCompleted ?? false);

    public AbstractStage(string stageName, Pipeline pipeline)
    {
        StageName = stageName;
        ParentPipeline = pipeline;
        ParentPipeline.RegisterStage(this);
    }

    public AbstractStage(string stageName, Pipeline pipeline, TimeSpan timeout)
    {
        StageName = stageName;
        ParentPipeline = pipeline;
        ParentPipeline.RegisterStage(this);
        TimeoutTask = Task.Delay(timeout);
    }

    public AbstractStage AddChildStage(AbstractStage childStage)
    {
        _ = childStage.ParentStageIds.Add(StageId);
        _ = ChildStageIds.Add(childStage.StageId);

        return childStage;
    }

    public bool ParentStagesComplete(List<string> completedStages)
    {
        return ParentStageIds.All(id => completedStages.Contains(id));
    }

    public List<string> GetParentStageIds()
        => ParentStageIds.ToList();

    protected abstract Task<StageResult> ProcessAsync(PipelineContext context);
}