using ABN.BciCommon.Stages.Base;

namespace ABN.BciCommon.Stages;

public class DelayStage(string stageName, Pipeline pipeline, TimeSpan delay) : AbstractStage(stageName, pipeline)
{
    private TimeSpan _delay = delay;

    protected override async Task<StageResult> ProcessAsync(PipelineContext context)
    {
        await Task.Delay(_delay);
        return StageResult.Succeeded;
    }

    public static DelayStage Seconds(string stageName, Pipeline pipeline, float seconds)
        => new DelayStage(stageName, pipeline, TimeSpan.FromSeconds(seconds));
}