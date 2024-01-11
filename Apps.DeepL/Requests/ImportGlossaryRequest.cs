using Blackbird.Applications.Sdk.Common;
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
        [Display("Glossary")]
        public FileReference File {  get; set; }

        [Display("New glossary name")]
        public string? Name { get; set; }
    }
}
