﻿using Blackbird.Applications.Sdk.Common;
using Apps.DeepL.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.DeepL.Requests;

public class DocumentTranslationRequest
{
    public FileReference File { get; set; }

    [Display("Source language")]
    [DataSource(typeof(SourceLanguageDataHandler))]
    public string? SourceLanguage { get; set; }

    [Display("Target language")]
    [DataSource(typeof(TargetLanguageDataHandler))]
    public string TargetLanguage { get; set; }

    [Display("Formal")]
    public bool? Formal { get; set; }

    [Display("Glossary")]
    [DataSource(typeof(GlossariesDataHandler))]
    public string? GlossaryId { get; set; }

    [Display("Translate file name")]
    public bool? TranslateFileName { get; set; }
}