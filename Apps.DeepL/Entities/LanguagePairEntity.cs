using Blackbird.Applications.Sdk.Common;

namespace Apps.DeepL.Entities;

public class LanguagePairEntity
{
    [Display("Source language")]
    public string SourceLanguage { get; set; }
    
    [Display("Target language")]
    public string TargetLanguage { get; set; }
}