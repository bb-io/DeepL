using Blackbird.Applications.Sdk.Common;
using Apps.DeepL.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.DeepL.Requests;

public class TextTranslationRequest
{
    [Display("Text", Description = "Text to translate")]
    public string Text { get; set; }

    [Display("Source language", Description = "The source language for translation")]
    [DataSource(typeof(SourceLanguageDataHandler))]
    public string? SourceLanguage { get; set; }

    [Display("Target language", Description = "The target language for translation")]
    [DataSource(typeof(TargetLanguageDataHandler))]
    public string TargetLanguage { get; set; }

    [Display("Formal", Description = "Indicates whether the translation should be formal")]
    public bool? Formal { get; set; }

    [Display("Glossary", Description = "The ID of the glossary to be used for translation")]
    [DataSource(typeof(GlossariesDataHandler))]
    public string? GlossaryId { get; set; }

    [Display("Tag handling",
        Description = "Specifies how tags in the text should be handled during translation")]
    public string? TagHandling { get; set; }

    // Todo: Split sentences

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
}