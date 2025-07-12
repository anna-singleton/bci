using ABN.BciCommon.Stages.Base;

using Microsoft.Extensions.Logging;

namespace ABN.BciCommon.Stages;

public class DumpContextStage(Pipeline pipeline) : AbstractStage("DumpContext", pipeline)
{
    protected override Task<StageResult> ProcessAsync(PipelineContext context)
    {
        var contextEntries = context.DumpContext();
        Logger.LogInformation("Current Context: {Context}", contextEntries);

        return Task.FromResult(StageResult.Succeeded);
    }
}