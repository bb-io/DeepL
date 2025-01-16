using Apps.DeepL.DataSourceHandlers.Enums;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.DeepL.Requests;
public class ImproveRequest
{
    [Display("Text")]
    public string Text { get; set; }

    [Display("Language")]
    [StaticDataSource(typeof(ImproveLanguageDataHandler))]
    public string? TargetLanguage { get; set; }

    [Display("Writing style")]
    [StaticDataSource(typeof(WritingStyleDataHandler))]
    public string? WritingStyle { get; set; }

    [Display("Tone")]
    [StaticDataSource(typeof(ToneDataHandler))]
    public string? Tone { get; set; }
}
