using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.DeepL.Responses;
public class ImproveResponse
{
    [Display("Improved text")]
    [JsonProperty("text")]
    public string Text { get; set; }

    [Display("Language")]
    [JsonProperty("target_language")]
    public string TargetLanguage { get; set; }

    [Display("Detected source language")]
    [JsonProperty("detected_source_language")]
    public string DetectedSourceLanguage { get; set; }
}
