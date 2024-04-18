using Apps.DeepL.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.DeepL.Requests.Glossaries;

public class CreateGlossaryRequest
{
    public string Name { get; set; }

    [Display("Source language")]
    [StaticDataSource(typeof(GlossaryLanguageDataHandler))]
    public string SourceLanguageCode { get; set; }

    [Display("Target language")]
    [StaticDataSource(typeof(GlossaryLanguageDataHandler))]
    public string TargetLanguageCode { get; set; }

    [Display("TSV/CSV file")] public FileReference File { get; set; }
}