using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;

namespace Apps.DeepL.Responses;

public class FileResponse : ITranslateFileOutput
{
    public FileReference File { get; set; } = default!;

    [Display("Billed characters", Description = "The amount of characters that DeepL translated")]
    public int? BilledCharacters { get; set; }
}