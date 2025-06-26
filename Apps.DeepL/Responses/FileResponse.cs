using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;

namespace Apps.DeepL.Responses;

public class FileResponse : ITranslateFileOutput
{
    public FileReference File { get; set; } = default!;
}