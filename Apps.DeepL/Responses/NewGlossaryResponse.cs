using Blackbird.Applications.Sdk.Common;

namespace Apps.DeepL.Responses;

public class NewGlossaryResponse
{
    [Display("Glossary ID")]
    public string GossaryId { get; set; }

    [Display("Name")]
    public string Name { get; set; }

    [Display("Source language")]
    public string SourceLanguageCode { get; set; }

    [Display("Target language")]
    public string TargetLanguageCode { get; set; }

    [Display("Entry count")]
    public int EntryCount { get; set; }

    [Display("Warnings")]
    public List<string> Warnings { get; set; } = new List<string>();
}