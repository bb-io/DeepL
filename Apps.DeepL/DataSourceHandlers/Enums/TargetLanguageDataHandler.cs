using Apps.DeepL.Constants;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.DeepL.DataSourceHandlers.Enums;

public class TargetLanguageDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return LanguagesConstants.TargetLanguages.Select(x => new DataSourceItem(x.Key, x.Value));
    }
}