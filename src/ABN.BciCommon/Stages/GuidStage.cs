using ABN.BciCommon.Stages.Base;

using Microsoft.Extensions.Logging;

namespace ABN.BciCommon.Stages;

public class GuidStage(ILogger logger) : AbstractStage(logger, "GuidStage")
{
    protected override ValueTask<bool> ShouldExecuteAsync(PipelineContext context)
    {
        return ValueTask.FromResult(true);
    }

    protected override Task<StageResult> ProcessAsync(PipelineContext context)
    {
        var guid = Guid.NewGuid().ToString();
        var newGuidIdx = context.Variables.Count(kv => kv.Key.StartsWith("guid-"));
        Logger.LogInformation("generated {Guid} for Index {Idx}", guid, newGuidIdx);
        context.Variables[$"guid-{newGuidIdx}"] = guid;
        return Task.FromResult(StageResult.Succeeded);
    }
}