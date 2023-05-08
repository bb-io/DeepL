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
        public string? SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
    }
}
