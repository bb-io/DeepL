using Newtonsoft.Json;

namespace Apps.DeepL.Models
{
    public class CreateGlossaryV3Result
    {
        [JsonProperty("glossary_id")]
        public string GlossaryId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("dictionaries")]
        public DictionaryInfo[] Dictionaries { get; set; }
    }

    public class DictionaryInfo
    {
        [JsonProperty("source_lang")]
        public string SourceLang { get; set; }

        [JsonProperty("target_lang")]
        public string TargetLang { get; set; }

        [JsonProperty("entry_count")]
        public int EntryCount { get; set; }
    }
}
