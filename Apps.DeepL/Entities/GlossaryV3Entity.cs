using Newtonsoft.Json;

namespace Apps.DeepL.Entities;

public class GlossaryV3Entity
{
    [JsonProperty("glossary_id")]
    public string GlossaryId { get; set; }

    public string Name { get; set; }
}
