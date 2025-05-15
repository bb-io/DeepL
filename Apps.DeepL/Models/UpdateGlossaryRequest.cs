using Apps.DeepL.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.DeepL.Models
{
    public class UpdateGlossaryRequest:ImportGlossaryRequest
    {
        [Display("Glossary ID", Description = "The ID of the glossary to update")]
        public string GlossaryId { get; set; }

    }
}
