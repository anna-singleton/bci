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
        var headStage = new GuidStage(logger);
        var pipeline = new Pipeline(logger, headStage);


        var childStage = new GuidStage(logger);
        pipeline.AddStageAsChild(childStage, headStage.StageId);

        await pipeline.ExecuteAsync();
    }
}