using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.DeepL.DataSourceHandlers.Enums;

public class ModelTypeDataHandler : IStaticDataSourceItemHandler
{
    private static Dictionary<string, string> Data => new()
    {
        { "latency_optimized", "(Classic) Optimized for speed" },
        { "quality_optimized", "(Next-gen) Optimized for quality" },
        { "prefer_quality_optimized", "(Next-gen) Best quality (recommended)" }
    };

    public IEnumerable<DataSourceItem> GetData()
    {
        return Data.Select(x => new DataSourceItem(x.Key, x.Value));
    }
}
