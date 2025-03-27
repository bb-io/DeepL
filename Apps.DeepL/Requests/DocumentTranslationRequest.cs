using Blackbird.Applications.Sdk.Common;
using Apps.DeepL.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Apps.DeepL.DataSourceHandlers.Enums;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.DeepL.Requests;

public class DocumentTranslationRequest
{
    public FileReference File { get; set; } = default!;

    [Display("Source language"), StaticDataSource(typeof(SourceLanguageDataHandler))]
    public string? SourceLanguage { get; set; }

    [Display("Target language"), StaticDataSource(typeof(TargetLanguageDataHandler))]
    public string TargetLanguage { get; set; } = string.Empty;

    [Display("Formality", Description = "Indicates whether the translation should be formal")]
    [StaticDataSource(typeof(FormalityDataHandler))]
    public string? Formality { get; set; }

    [Display("Glossary")]
    [DataSource(typeof(GlossariesDataHandler))]
    public string? GlossaryId { get; set; }

    [Display("Translate file name")]
    public bool? TranslateFileName { get; set; }
}