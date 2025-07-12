namespace ABN.BciCommon;

public class PipelineContext
{
    public Dictionary<string, object> Variables = [];

    public string DumpContext()
    {
        return string.Join(", ", Variables.Select(kv => KVPairToString(kv)));
    }

    private string KVPairToString(KeyValuePair<string, object> kv)
    {
        return $"{kv.Key}: {kv.Value}";
    }
}