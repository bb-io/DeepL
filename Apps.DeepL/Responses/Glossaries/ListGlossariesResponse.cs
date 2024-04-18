using Apps.DeepL.Entities;

namespace Apps.DeepL.Responses.Glossaries;

public record ListGlossariesResponse(IEnumerable<GlossaryEntity> Glossaries);