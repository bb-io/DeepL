using Apps.DeepL.Entities;

namespace Apps.DeepL.Responses.Glossaries;

public record ListGlossariesV3Response(IEnumerable<GlossaryV3Entity> Glossaries);
