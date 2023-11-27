using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.DeepL.Requests
{
    public class CreateGlossaryRequest
    {
        public string Name { get; set; }

        [Display("Source language")]
        public string SourceLanguage { get; set; }

        [Display("Target language")]
        public string TargetLanguage { get; set; }

        public File File { get; set; }

    }
}
