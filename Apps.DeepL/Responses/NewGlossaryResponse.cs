using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.DeepL.Responses
{
    public class NewGlossaryResponse
    {
        [Display("Glossary ID")]
        public string GossaryId { get; set; }

        public string Name { get; set; }

        [Display("Source language")]
        public string SourceLanguageCode { get; set; }

        [Display("Target language")]
        public string TargetLanguageCode { get; set; }

        [Display("Entry count")]
        public int EntryCount { get; set; }
    }
}
