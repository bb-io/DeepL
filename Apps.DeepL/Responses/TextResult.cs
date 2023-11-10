using Blackbird.Applications.Sdk.Common;

namespace Apps.DeepL.Responses;

public class TextResponse
{
    [Display("Translated text", Description = "The text after translation")]
    public string TranslatedText { get; set; }

    [Display("Detected source language", Description = "The language detected in the source text")]
    public string DetectedSourceLanguage { get; set; }
}