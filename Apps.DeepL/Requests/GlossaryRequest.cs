using Apps.DeepL.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.DeepL.Requests;

public class GlossaryRequest
{
    [Display("Glossary")]
    [DataSource(typeof(GlossariesDataHandler))]
    public string GlossaryId { get; set; }
}