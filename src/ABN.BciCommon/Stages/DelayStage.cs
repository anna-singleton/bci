using ABN.BciCommon.Stages.Base;

using Microsoft.Extensions.Logging;

namespace ABN.BciCommon.Stages;

public class DelayStage(ILogger logger, string stageName, TimeSpan delay) : AbstractStage(logger, stageName)
{
    private TimeSpan _delay = delay;

    protected override async Task<StageResult> ProcessAsync(PipelineContext context)
    {
        await Task.Delay(_delay);
        return StageResult.Succeeded;
    }

    public static DelayStage Seconds(ILogger logger, string stageName, float seconds)
        => new DelayStage(logger, stageName, TimeSpan.FromSeconds(seconds));
}