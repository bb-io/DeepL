using Apps.DeepL.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.DeepL.Models;
public class WriteResponseDto
{
    [JsonProperty("improvements")]
    public List<ImproveResponse> Improvements { get; set;  }
}
