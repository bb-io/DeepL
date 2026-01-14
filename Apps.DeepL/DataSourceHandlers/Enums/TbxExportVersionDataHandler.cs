using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.DeepL.DataSourceHandlers.Enums
{
    public class TbxExportVersionDataHandler : IStaticDataSourceItemHandler
    {
        private static Dictionary<string, string> Data => new()
        {
            { "v3", "TBX v3" },
            { "v2", "TBX v2" },
        };

        public IEnumerable<DataSourceItem> GetData()
            => Data.Select(x => new DataSourceItem(x.Key, x.Value));
    }
}
