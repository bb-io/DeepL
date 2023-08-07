using Blackbird.Applications.Sdk.Common;

namespace Apps.DeepL.Responses
{
    public class TextResponse
    {
        [Display("Translated text")]
        public string TranslatedText { get; set; }

        [Display("Detected source language")]
        public string DetectedSourceLanguage { get; set; }
    }
}
