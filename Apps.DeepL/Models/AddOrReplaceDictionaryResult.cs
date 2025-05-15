using Newtonsoft.Json;

namespace Apps.DeepL.Models
{
    public class AddOrReplaceDictionaryResult
    {
        [JsonProperty("source_lang")]
        public string SourceLang { get; set; }

        [JsonProperty("target_lang")]
        public string TargetLang { get; set; }

        [JsonProperty("entry_count")]
        public int EntryCount { get; set; }
    }
}
