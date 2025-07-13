using System.Collections.Concurrent;

using ABN.BciCommon.Stages.Base;

using Microsoft.Extensions.Logging;

namespace ABN.BciCommon;

public class Pipeline
{
    public ILogger Logger { get; }

    private readonly Dictionary<string, AbstractStage> _stages;

    public Pipeline(ILogger logger)
    {
        Logger = logger;
        _stages = [];
    }

    public async Task ExecuteAsync()
    {
        var context = new PipelineContext();
        while (GetPendingStageIds().Count > 0)
        {
            var pending_stages = GetPendingStageIds();
            foreach (var stageId in pending_stages)
            {
                var stage = _stages[stageId];
                if (stage.IsTimedOut)
                {
                    stage.State = StageState.TimedOut;
                    stage.CancelStage();
                    continue;
                }

                if (AnyStagesMatchPredicate(stage.GetParentStageIds(), s => s.IsFailed || s.IsSkipped || s.IsTimedOut))
                {
                    stage.State = StageState.Skipped;
                    continue;
                }

                if (AllStagesMatchPredicate(stage.GetParentStageIds(), s => s.IsSuccessful))
                {
                    await stage.StartStage(context);
                    continue;
                }
            }
        }

        while (_stages.Any(s => s.Value.IsActive))
        {
            foreach (var stage in _stages.Where(s => s.Value.IsActive))
            {
                if (stage.Value.IsTimedOut)
                {
                    stage.Value.State = StageState.TimedOut;
                }
            }
        }
    }

    public AbstractStage RegisterStage(AbstractStage stage)
    {
        if (_stages.ContainsKey(stage.StageId))
        {
            return stage;
        }

        _stages.Add(stage.StageId, stage);

        return stage;
    }

    public bool AllStagesMatchPredicate(List<string> Ids, Func<AbstractStage, bool> f)
    {
        return Ids
            .Select(id => _stages[id])
            .All(f);
    }

    public bool AnyStagesMatchPredicate(List<string> Ids, Func<AbstractStage, bool> f)
    {
        return Ids
            .Select(id => _stages[id])
            .Any(f);
    }

    public List<string> GetAllStageIds()
        => _stages.Select(kv => kv.Value.StageId).ToList();

    private List<string> GetPendingStageIds()
        => GetStageIdsByPredicate(s => s.IsPending);

    private List<string> GetActiveStageIds()
        => GetStageIdsByPredicate(s => s.IsActive);

    private List<string> GetFinishedStageIds()
        => GetStageIdsByPredicate(s => s.IsFinished);

    private List<string> GetSuccessfulStageIds()
        => GetStageIdsByPredicate(s => s.IsSuccessful);

    private List<string> GetFailedStageIds()
        => GetStageIdsByPredicate(s => s.IsFailed);

    private List<string> GetStageIdsByPredicate(Func<AbstractStage, bool> f)
    {
        return  _stages
            .Select((kv) => kv.Value)
            .Where(f)
            .Select(s => s.StageId)
            .ToList();
    }
}