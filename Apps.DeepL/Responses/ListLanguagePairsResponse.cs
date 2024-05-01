using Apps.DeepL.Entities;
using Blackbird.Applications.Sdk.Common;

namespace Apps.DeepL.Responses;

public class ListLanguagePairsResponse
{
    [Display("Language pairs")]
    public IEnumerable<LanguagePairEntity> LanguagePairs { get; set; }
}