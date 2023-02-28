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
        public string? SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
    }
}
