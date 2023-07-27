using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
