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
        Logger.LogInformation("generated {Guid}", guid);
        context.Variables["guid"] = guid;
        return Task.FromResult(StageResult.Succeeded);
    }
}