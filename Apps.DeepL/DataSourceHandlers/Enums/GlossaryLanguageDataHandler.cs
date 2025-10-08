using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.DeepL.DataSourceHandlers.Enums;

public class GlossaryLanguageDataHandler : IStaticDataSourceItemHandler
{
    private static Dictionary<string, string> Data => new()
    {
        { "de", "German" },
        { "en", "English" },
        { "en-us", "English (American)" },
        { "en-gb", "English (British)" },
        { "es", "Spanish" },
        { "fr", "French" },
        { "it", "Italian" },
        { "ja", "Japanese" },
        { "nl", "Dutch" },
        { "pl", "Polish" },
        { "pt", "Portuguese" },
        { "pt-br", "Portuguese (Brazilian)" },
        { "pt-pt", "Portuguese (all Portuguese variants excluding Brazilian Portuguese)" },
        { "ru", "Russian" },
        { "zh", "Chinese" },
        { "zh-hans", "Chinese (simplified)" },
        { "zh-hant", "Chinese (traditional)" },{ "ar", "Arabic" },
        { "bg", "Bulgarian" },
        { "cs", "Czech" },
        { "da", "Danish" },
        { "el", "Greek" },
        { "et", "Estonian" },
        { "fi", "Finnish" },
        { "he", "Hebrew" },
        { "hu", "Hungarian" },
        { "id", "Indonesian" },
        { "ko", "Korean" },
        { "lt", "Lithuanian" },
        { "lv", "Latvian" },
        { "nb", "Norwegian Bokmål" },
        { "ro", "Romanian" },
        { "sk", "Slovak" },
        { "sl", "Slovenian" },
        { "sv", "Swedish" },
        { "th", "Thai" },
        { "tr", "Turkish" },
        { "uk", "Ukrainian" },
        { "vi", "Vietnamese" }
    };
    
    public IEnumerable<DataSourceItem> GetData()
    {
        return Data.Select(x => new DataSourceItem(x.Key, x.Value));
    }
}