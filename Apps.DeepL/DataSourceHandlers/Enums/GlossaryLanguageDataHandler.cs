using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.DeepL.DataSourceHandlers.Enums;

public class GlossaryLanguageDataHandler : IStaticDataSourceHandler
{
    public Dictionary<string, string> GetData() => new()
    {
        { "de", "German" },
        { "en", "English" },
        { "es", "Spanish" },
        { "fr", "French" },
        { "it", "Italian" },
        { "ja", "Japanese" },
        { "nl", "Dutch" },
        { "pl", "Polish" },
        { "pt", "Portuguese" },
        { "ru", "Russian" },
        { "zh", "Chinese" },
    };
}