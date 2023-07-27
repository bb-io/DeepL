using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.DeepL.Requests
{
    public class TextTranslationRequest
    {
        public string Text { get; set; }

        [Display("Source language")]
        public string? SourceLanguage { get; set; }

        [Display("Target language")]
        public string TargetLanguage { get; set; }

        [Display("Formal")]
        public bool? Formal { get; set; }

        [Display("Glossary")]
        public string? GlossaryId { get; set; }

        [Display("Tag handling")]
        public string? TagHandling { get; set; }

        // Todo: Split sentences

        [Display("Preserve formatting")]
        public bool? PreserveFormatting { get; set; }

        [Display("Outline detection")]
        public bool? OutlineDetection { get; set; }

        [Display("Non-splitting tags")]
        public List<string>? NonSplittingTags { get; set; }

        [Display("Splitting tags")]
        public List<string>? SplittingTags { get; set; }

        [Display("Ignore tags")]
        public List<string>? IgnoreTags { get; set; }

    }
}
