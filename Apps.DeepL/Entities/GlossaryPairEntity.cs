namespace Apps.DeepL.Entities;

public record GlossaryPairEntity(string SourceLang, string TargetLang)
{
    public static GlossaryPairEntity Of(string sourceLang, string targetLang)
    {
        return new(sourceLang.ToLowerInvariant(), targetLang.ToLowerInvariant());
    }
}