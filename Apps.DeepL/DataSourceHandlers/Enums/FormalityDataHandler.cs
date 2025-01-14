using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.DeepL.DataSourceHandlers.Enums
{
    public class FormalityDataHandler : IStaticDataSourceItemHandler
    {
        private static Dictionary<string, string> Data => new()
        {
            { "default", "Default" },
            { "more", "More" },
            { "less", "Less" },
            { "prefer_more", "Prefer more" },
            { "prefer_less", "Prefer less" },
        };

        public IEnumerable<DataSourceItem> GetData()
        {
            return Data.Select(x => new DataSourceItem(x.Key, x.Value));
        }
    }
}
