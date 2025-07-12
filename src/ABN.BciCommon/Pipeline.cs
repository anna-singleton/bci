using System.Collections.Concurrent;

using ABN.BciCommon.Stages.Base;

using Microsoft.Extensions.Logging;

namespace ABN.BciCommon;

public class Pipeline
{
    private readonly ILogger _logger;

    private readonly Dictionary<string, AbstractStage> _stages;

    public Pipeline(ILogger logger)
    {
        _logger = logger;
        _stages = [];
    }

    public async Task ExecuteAsync()
    {
        var context = new PipelineContext();
        ConcurrentBag<Task> tasks = [];
        while (GetPendingStageIds().Count > 0)
        {
            var pending_stages = GetPendingStageIds();
            foreach (var stageId in pending_stages)
            {
                var stage = _stages[stageId];
                if (stage.ParentStagesComplete(GetFinishedStageIds()))
                {
                    tasks.Add(stage.ExecuteAsync(context));
                }
            }
        }

        foreach (var task in tasks)
        {
            await task;
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

    private List<string> GetPendingStageIds()
        => GetStageIdsByPredicate(s => s.IsPending);

    private List<string> GetActiveStageIds()
        => GetStageIdsByPredicate(s => s.IsActive);

    private List<string> GetFinishedStageIds()
        => GetStageIdsByPredicate(s => s.IsFinished);

    private List<string> GetStageIdsByPredicate(Func<AbstractStage, bool> f)
    {
        return  _stages
            .Select((kv) => kv.Value)
            .Where(f)
            .Select(s => s.StageId)
            .ToList();
    }
}