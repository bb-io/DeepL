using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.DeepL.Requests
{
    public class DocumentTranslationRequest
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }

        [Display("Source language")]
        public string? SourceLanguage { get; set; }

        [Display("Target language")]
        public string TargetLanguage { get; set; }

        [Display("Formal")]
        public bool? Formal { get; set; }

        [Display("Glossary")]
        public string? GlossaryId { get; set; }
    }
}
