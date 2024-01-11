using Apps.DeepL.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.DeepL.Requests
{
    public class ImportGlossaryRequest
    {
        [Display("Glossary", Description = "Glossary file exported from other Blackbird apps")]
        public FileReference File {  get; set; }

        [Display("New glossary name", Description = "You can override glossary name which is set in a file")]
        public string? Name { get; set; }

        [Display("Source language", Description = "DeepL glossary structure is key-value. Key is your source language.\nMake sure exported glossary has this language")]
        [DataSource(typeof(SourceLanguageDataHandler))]
        public string SourceLanguageCode { get; set; }

        [Display("Target language",  Description = "DeepL glossary structure is key-value. Value is your target language.\nMake sure exported glossary has this language")]
        [DataSource(typeof(TargetLanguageDataHandler))]
        public string TargetLanguageCode { get; set; }
    }
}
