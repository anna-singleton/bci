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

        var headStage = pipeline.RegisterStage(new GuidStage(logger, pipeline));

        var childGuidStage = headStage.AddChildStage(new GuidStage(logger, pipeline));

        var delay = headStage.AddChildStage(DelayStage.Seconds(logger, "Wait 5s", pipeline, 5.0f));

        var dumpContextStage = delay.AddChildStage(new DumpContextStage(logger, pipeline));

        await pipeline.ExecuteAsync();
    }
}