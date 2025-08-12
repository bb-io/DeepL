using Apps.DeepL.DataSourceHandlers;
using Apps.DeepL.DataSourceHandlers.Enums;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.DeepL.Requests.Xliff;

public class XliffTranslationRequest
{
    [Display("XLIFF file")]
    public FileReference File { get; set; } = default!;

    [Display("Source language", Description = "The source language for translation"), StaticDataSource(typeof(SourceLanguageDataHandler))]
    public string? SourceLanguage { get; set; }

    [Display("Target language", Description = "The target language for translation"), StaticDataSource(typeof(TargetLanguageDataHandler))]
    public string TargetLanguage { get; set; } = string.Empty;

    [Display("Formality", Description = "Indicates whether the translation should be formal"), StaticDataSource(typeof(FormalityDataHandler))]
    public string? Formality { get; set; }

    [Display("Glossary", Description = "The ID of the glossary to be used for translation"), DataSource(typeof(GlossariesDataHandler))]
    public string? GlossaryId { get; set; }

    [Display("Tag handling",
        Description = "Specifies how tags in the text should be handled during translation")]
    public string? TagHandling { get; set; }

    [Display("Model type", Description = "Specifies which DeepL model should be used for translation"), StaticDataSource(typeof(ModelTypeDataHandler))]
    public string? ModelType { get; set; }

    [Display("Preserve formatting", Description = "Preserves the formatting of the text during translation")]
    public bool? PreserveFormatting { get; set; }

    [Display("Outline detection", Description = "Detects and preserves document outlines during translation")]
    public bool? OutlineDetection { get; set; }

    [Display("Non-splitting tags", Description = "Tags that should not be split during translation")]
    public List<string>? NonSplittingTags { get; set; }

    [Display("Splitting tags", Description = "Tags that can be split during translation")]
    public List<string>? SplittingTags { get; set; }

    [Display("Ignore tags", Description = "Tags that should be ignored during translation")]
    public List<string>? IgnoreTags { get; set; }

    [Display("Context")]
    public string? Context { get; set; }
    
    [Display("Use batch processing", Description = "Process translation units in batches if enabled, otherwise process one by one")]
    public bool? UseBatches { get; set; }

    [Display("Translate only empty units (off by default)", Description = "When enabled, only units without existing translations will be processed.")]
    public bool? TranslateOnlyEmptyUnits { get; set; }
}
