using Apps.DeepL.DataSourceHandlers;
using Apps.DeepL.DataSourceHandlers.Enums;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.DeepL.Models
{
    public class ExportUniversalGlossaryRequest
    {
        [Display("Glossary ID")]
        [DataSource(typeof(GlossariesDataHandler))]
        public string GlossaryId { get; set; } = default!;

        [Display("TBX export version")]
        [StaticDataSource(typeof(TbxExportVersionDataHandler))]
        public string? Version { get; set; }
    }
}
