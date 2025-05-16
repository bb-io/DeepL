using Apps.DeepL.DataSourceHandlers;
using Apps.DeepL.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.DeepL.Models
{
    public class UpdateGlossaryRequest : ImportGlossaryRequest
    {
        [Display("Glossary ID", Description = "The ID of the glossary to update")]
        [DataSource(typeof(GlossariesDataHandler))]
        public string GlossaryId { get; set; }
    }
}
