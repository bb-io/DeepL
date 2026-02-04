using Apps.DeepL.Entities;
using Newtonsoft.Json;

namespace Apps.DeepL.Responses.StyleRules;
public class ListStyleRulesResponse
{
    [JsonProperty("style_rules")]
    public IEnumerable<StyleRuleEntity> StyleRules { get; set; }
}
