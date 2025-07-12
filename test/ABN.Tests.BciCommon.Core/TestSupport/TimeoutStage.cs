using ABN.BciCommon;
using ABN.BciCommon.Stages.Base;

namespace ABN.Tests.BciCommon.TestSupport;

public class TimeoutStage : AbstractStage
{
    public TimeoutStage(Pipeline pipeline) : base("TimeoutStage", pipeline, TimeSpan.FromSeconds(0.5))
    {
    }

    protected override async Task<StageResult> ProcessAsync(PipelineContext context)
    {
        await Task.Delay(TimeSpan.FromSeconds(5));

        return StageResult.Succeeded;
    }
}