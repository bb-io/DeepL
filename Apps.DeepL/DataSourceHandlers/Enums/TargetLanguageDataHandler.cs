﻿using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.DeepL.DataSourceHandlers.Enums;

public class TargetLanguageDataHandler : IStaticDataSourceHandler
{
    public Dictionary<string, string> GetData() => new()
    {
        { "AR", "Arabic" },
        { "BG", "Bulgarian" },
        { "CS", "Czech" },
        { "DA", "Danish" },
        { "DE", "German" },
        { "EL", "Greek" },
        { "EN-GB", "English (British)" },
        { "EN-US", "English (American)" },
        { "ES", "Spanish" },
        { "ET", "Estonian" },
        { "FI", "Finnish" },
        { "FR", "French" },
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
        { "PT-BR", "Portuguese (Brazilian)" },
        { "PT-PT", "Portuguese (Portiguese)" },
        { "RO", "Romanian" },
        { "RU", "Russian" },
        { "SK", "Slovak" },
        { "SL", "Slovenian" },
        { "SV", "Swedish" },
        { "TR", "Turkish" },
        { "UK", "Ukrainian" },
        { "ZH-HANS", "Chinese (simplified)" },
        { "ZH-HANT", "Chinese (traditional)" },
    };
}