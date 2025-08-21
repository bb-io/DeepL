using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.DeepL.DataSourceHandlers.Enums;

public class DeepLProcessFileFormatHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return new List<DataSourceItem>
        {
            new ("application/xliff+xml", "Interoperable XLIFF (default)"),
            new("xliff1", "XLIFF 1.2"),
            new ("original", "Original data format")
        };
    }
}