using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.DeepL.DataSourceHandlers.Enums;
public class OutputFileHandlingHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return new List<DataSourceItem> { 
            new DataSourceItem("xliff", "Interoperable XLIFF (default)"),
            new DataSourceItem("original", "Original data format"),
        };
    }
}
