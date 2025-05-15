using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Apps.DeepL.Models
{
    public class GetGlossaryEntriesResult
    {
        [JsonPropertyName("dictionaries")]
        public DictionaryEntries[] Dictionaries { get; set; }
    }
    public class DictionaryEntries
    {
        [JsonPropertyName("source_lang")]
        public string SourceLang { get; set; }

        [JsonPropertyName("target_lang")]
        public string TargetLang { get; set; }

        [JsonPropertyName("entries")]
        public string Entries { get; set; }

        [JsonPropertyName("entries_format")]
        public string EntriesFormat { get; set; }
    }
    public class GetGlossaryMetadataResult
    {
        [JsonPropertyName("glossary_id")]
        public string GlossaryId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("creation_time")]
        public DateTime CreationTime { get; set; }

        [JsonPropertyName("dictionaries")]
        public DictionaryInfo[] Dictionaries { get; set; }
    }
}
