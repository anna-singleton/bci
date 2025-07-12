using ABN.BciCommon.Stages.Base;

using Microsoft.Extensions.Logging;

namespace ABN.BciCommon.Stages;

public class DelayStage(ILogger logger, string stageName, Pipeline pipeline, TimeSpan delay) : AbstractStage(logger, stageName, pipeline)
{
    private TimeSpan _delay = delay;

    protected override async Task<StageResult> ProcessAsync(PipelineContext context)
    {
        await Task.Delay(_delay);
        return StageResult.Succeeded;
    }

    public static DelayStage Seconds(ILogger logger, string stageName, Pipeline pipeline, float seconds)
        => new DelayStage(logger, stageName, pipeline, TimeSpan.FromSeconds(seconds));
}