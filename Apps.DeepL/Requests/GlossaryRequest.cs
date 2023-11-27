using Apps.DeepL.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.DeepL.Requests
{
    public class GlossaryRequest
    {
        [Display("Glossary")]
        [DataSource(typeof(GlossariesDataHandler))]
        public string GlossaryId { get; set; }
    }
}
