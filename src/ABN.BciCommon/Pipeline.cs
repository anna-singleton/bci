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
        ConcurrentBag<Task> active_tasks = [];
        active_tasks.Add(_headStage.ExecuteAsync(context));
        var pending_stages = _stages.Select(static kv => kv.Key).ToList();
        List<string> completed_stages = [];
        while (pending_stages.Count > 0)
        {
            for (var i = pending_stages.Count - 1; i >= 0; i--)
            {
                
            }
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
    }

    // private bool 
}