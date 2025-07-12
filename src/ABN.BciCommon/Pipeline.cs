using System.Collections.Concurrent;

using ABN.BciCommon.Stages.Base;

using Microsoft.Extensions.Logging;

namespace ABN.BciCommon;

public class Pipeline
{
    private readonly AbstractStage _headStage;

    private readonly ILogger _logger;

    private readonly Dictionary<string, AbstractStage> _stages;

    public Pipeline(ILogger logger, AbstractStage headStage)
    {
        _headStage = headStage;
        _logger = logger;
        _stages = [];
        _stages.Add(headStage.StageId, headStage);
    }

    public async Task ExecuteAsync()
    {
        var context = new PipelineContext();
        ConcurrentBag<Task> tasks = [];
        tasks.Add(_headStage.ExecuteAsync(context));
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

    public void AddStageAsChild(AbstractStage newStage, string parentStageId)
    {
        if (!_stages.TryGetValue(parentStageId, out var parentStage))
        {
            _logger.LogError("Could not add stage {StageId}.{StageName} as a child of {ParentStageId}, as the parent could not be found.",
                    newStage.StageId, newStage.StageName, parentStageId);
            return;
        }

        parentStage.AddChildStage(newStage);

        _stages.Add(newStage.StageId, newStage);
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