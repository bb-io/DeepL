using Apps.DeepL.DataSourceHandlers;
using Apps.DeepL.DataSourceHandlers.Enums;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Handlers;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;

namespace Apps.DeepL.Requests.Content;

public class ContentTranslationRequest : ITranslateFileInput
{
    [Display("Content file")]
    public FileReference File { get; set; } = default!;

    [Display("Source language", Description = "The source language for translation"), StaticDataSource(typeof(SourceLanguageDataHandler))]
    public string? SourceLanguage { get; set; }

    [Display("Target language", Description = "The target language for translation"), StaticDataSource(typeof(TargetLanguageDataHandler))]
    public string TargetLanguage { get; set; } = string.Empty;

    [Display("Formality", Description = "Indicates whether the translation should be formal"), StaticDataSource(typeof(FormalityDataHandler))]
    public string? Formality { get; set; }

    [Display("Glossary ID", Description = "The ID of the glossary to be used for translation"), DataSource(typeof(GlossariesDataHandler))]
    public string? GlossaryId { get; set; }

    [Display("Model type", Description = "Specifies which DeepL model should be used for translation"), StaticDataSource(typeof(ModelTypeDataHandler))]
    public string? ModelType { get; set; }

    [Display("Context")]
    public string? Context { get; set; }

    [Display("Preserve formatting", Description = "Preserves the formatting of the text during translation")]
    public bool? PreserveFormatting { get; set; }

    [Display("Output file handling", Description = "Determine the format of the output file. The default Blackbird behavior is to convert to XLIFF for future steps."), StaticDataSource(typeof(DeepLProcessFileFormatHandler))]
    public string? OutputFileHandling { get; set; }

    [Display("File translation strategy", Description = "Select whether to use DeepL's own file processing capabilities or use Blackbird interoperability mode"), StaticDataSource(typeof(FileTranslationStrategyHandler))]
    public string? FileTranslationStrategy { get; set; }
}
