using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.DeepL.DataSourceHandlers.Enums;

public class SourceLanguageDataHandler : IStaticDataSourceItemHandler
{
    private static Dictionary<string, string> Data => new()
    {
        { "AR", "Arabic" },
        { "BG", "Bulgarian" },
        { "CS", "Czech" },
        { "DA", "Danish" },
        { "DE", "German" },
        { "EL", "Greek" },
        { "EN", "English" },
        { "ES", "Spanish" },
        { "ET", "Estonian" },
        { "FI", "Finnish" },
        { "FR", "French" },
        { "HE", "Hebrew" },
        { "HU", "Hungarian" },
        { "ID", "Indonesian" },
        { "IT", "Italian" },
        { "JA", "Japanese" },
        { "KO", "Korean" },
        { "LT", "Lithuanian" },
        { "LV", "Latvian" },
        { "NB", "Norwegian Bokmål" },
        { "NL", "Dutch" },
        { "PL", "Polish" },
        { "PT", "Portuguese" },
        { "RO", "Romanian" },
        { "RU", "Russian" },
        { "SK", "Slovak" },
        { "SL", "Slovenian" },
        { "SV", "Swedish" },
        { "TH", "Thai" },
        { "TR", "Turkish" },
        { "UK", "Ukrainian" },
        { "VI", "Vietnamese" },
        { "ZH", "Chinese" },
    };
    
    public IEnumerable<DataSourceItem> GetData()
    {
        return Data.Select(x => new DataSourceItem(x.Key, x.Value));
    }
}