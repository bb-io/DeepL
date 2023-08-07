using Blackbird.Applications.Sdk.Common;
using Apps.DeepL.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.DeepL.Requests
{
    public class DocumentTranslationRequest
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }

        [Display("Source language")]
        [DataSource(typeof(LanguageDataHandler))]
        public string? SourceLanguage { get; set; }

        [Display("Target language")]
        [DataSource(typeof(LanguageDataHandler))]
        public string TargetLanguage { get; set; }

        [Display("Formal")]
        public bool? Formal { get; set; }

        [Display("Glossary")]
        public string? GlossaryId { get; set; }
    }
}
