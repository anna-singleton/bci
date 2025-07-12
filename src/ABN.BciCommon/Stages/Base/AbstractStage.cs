using Microsoft.Extensions.Logging;

namespace ABN.BciCommon.Stages.Base;

public abstract class AbstractStage(ILogger logger, string stageName, Pipeline pipeline)
{
    public StageState State { get; set; } = StageState.NotStarted;

    protected HashSet<string> ChildStageIds { get; } = [];

    protected HashSet<string> ParentStageIds { get; } = [];

    public string StageId { get; } = Guid.NewGuid().ToString();

    public string StageName { get; } = stageName;

    protected ILogger Logger { get; } = logger;

    private Pipeline ParentPipeline { get; } = pipeline;

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

    public AbstractStage AddChildStage(AbstractStage childStage)
    {
        _ = ChildStageIds.Add(childStage.StageId);
        _ = childStage.ParentStageIds.Add(StageId);
        ParentPipeline.RegisterStage(childStage);

        return childStage;
    }

    public bool ParentStagesComplete(List<string> completedStages)
    {
        return ParentStageIds.All(id => completedStages.Contains(id));
    }

    protected abstract Task<StageResult> ProcessAsync(PipelineContext context);
}