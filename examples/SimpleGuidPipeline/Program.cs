using ABN.BciCommon;
using ABN.BciCommon.Stages;

using Microsoft.Extensions.Logging;

namespace SimpleGuidPipeline;

public class Program
{
    public static async Task Main()
    {
        var logger = LoggerFactory.Create(static b => b
                .AddSimpleConsole(static options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                    })
                ).CreateLogger("Pipeline");
        logger.LogInformation("Executing Pipeline...");

        var pipeline = new Pipeline(logger);

        var headStage = new GuidStage(pipeline);

        var childGuidStage = headStage.AddChildStage(new GuidStage(pipeline));

        var delay = headStage.AddChildStage(DelayStage.Seconds("Wait 5s", pipeline, 5.0f));

        var dumpContextStage = delay.AddChildStage(new DumpContextStage(pipeline));

        await pipeline.ExecuteAsync();
    }
}