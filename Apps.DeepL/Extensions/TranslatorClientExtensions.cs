using Apps.DeepL.Entities;
using DeepL;

namespace Apps.DeepL.Extensions;

public static class TranslatorClientExtensions
{
    public static async Task<HashSet<GlossaryPairEntity>> GetSupportedGlossaryPairs(this Translator client)
    {
        var pairs = await client.GetGlossaryLanguagesAsync();
        return pairs.Select(p => GlossaryPairEntity.Of(p.SourceLanguageCode, p.TargetLanguageCode)).ToHashSet();
    }
}