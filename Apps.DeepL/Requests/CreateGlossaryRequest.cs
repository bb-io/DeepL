using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.DeepL.Requests;

public class CreateGlossaryRequest
{
    public string Name { get; set; }

    [Display("Source language")] public string SourceLanguage { get; set; }

    [Display("Target language")] public string TargetLanguage { get; set; }

    public FileReference File { get; set; }
}