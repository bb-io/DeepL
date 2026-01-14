using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.DeepL.Models
{
    public class ImportUniversalGlossaryRequest
    {
        [Display("File")]
        public FileReference File { get; set; } = default!;

        [Display("Name")]
        public string? Name { get; set; }

        [Display("Pivot language code")]
        public string? PivotLanguageCode { get; set; }

        [Display("Source language code")]
        public string? SourceLanguageCode { get; set; }

        [Display("Target language code")]
        public string? TargetLanguageCode { get; set; }

        [Display("Force header row (CSV/TSV)")]
        public bool? ForceHeaderRow { get; set; }
    }
}
