using Apps.DeepL.DataSourceHandlers;
using Apps.DeepL.DataSourceHandlers.Enums;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.DeepL.Requests.Content;

public class ContentTranslationRequest
{
    [Display("Content file")]
    public FileReference File { get; set; } = default!;

    [Display("Source language", Description = "The source language for translation"), StaticDataSource(typeof(SourceLanguageDataHandler))]
    public string? SourceLanguage { get; set; }

    [Display("Target language", Description = "The target language for translation"), StaticDataSource(typeof(TargetLanguageDataHandler))]
    public string TargetLanguage { get; set; } = string.Empty;

    [Display("Formality", Description = "Indicates whether the translation should be formal"), StaticDataSource(typeof(FormalityDataHandler))]
    public string? Formality { get; set; }

    [Display("Glossary", Description = "The ID of the glossary to be used for translation"), DataSource(typeof(GlossariesDataHandler))]
    public string? GlossaryId { get; set; }

    [Display("Model type", Description = "Specifies which DeepL model should be used for translation"), StaticDataSource(typeof(ModelTypeDataHandler))]
    public string? ModelType { get; set; }

    [Display("Context")]
    public string? Context { get; set; }
}
