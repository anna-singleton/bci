using ABN.BciCommon;
using ABN.BciCommon.Stages.Base;

namespace ABN.Tests.BciCommon.TestSupport;

public class FailureStage : AbstractStage
{
    public FailureStage(Pipeline pipeline) : base("failedStage", pipeline)
    {

    }

    protected override Task<StageResult> ProcessAsync(PipelineContext context)
    {
        return Task.FromResult(StageResult.Failed);
    }
}