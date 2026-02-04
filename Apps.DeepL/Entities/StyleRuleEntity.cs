using Newtonsoft.Json;

namespace Apps.DeepL.Entities;
public class StyleRuleEntity
{
    [JsonProperty("style_id")]
    public string StyleId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("language")]
    public string Language { get; set; }

    [JsonProperty("version")]
    public int Version { get; set; }
}
