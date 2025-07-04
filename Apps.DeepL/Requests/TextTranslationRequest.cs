﻿using Blackbird.Applications.Sdk.Common;
using Apps.DeepL.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Apps.DeepL.DataSourceHandlers.Enums;
using Apps.DeepL.Requests.Xliff;
using Blackbird.Xliff.Utils.Models;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;

namespace Apps.DeepL.Requests;

public class TextTranslationRequest : ITranslateTextInput
{
    [Display("Text", Description = "Text to translate")]
    public string Text { get; set; }

    [Display("Source language", Description = "The source language for translation"), StaticDataSource(typeof(SourceLanguageDataHandler))]
    public string? SourceLanguage { get; set; }

    [Display("Target language", Description = "The target language for translation"), StaticDataSource(typeof(TargetLanguageDataHandler))]
    public string TargetLanguage { get; set; }

    [Display("Formality", Description = "Indicates whether the translation should be formal"), StaticDataSource(typeof(FormalityDataHandler))]
    public string? Formality { get; set; }

    [Display("Glossary", Description = "The ID of the glossary to be used for translation"), DataSource(typeof(GlossariesDataHandler))]
    public string? GlossaryId { get; set; }

    [Display("Tag handling",
        Description = "Specifies how tags in the text should be handled during translation")]
    public string? TagHandling { get; set; }

    [Display("Model type", Description = "Specifies which DeepL model should be used for translation"), StaticDataSource(typeof(ModelTypeDataHandler))]
    public string? ModelType { get; set; }

    // Todo: Split sentences

    [Display("Preserve formatting", Description = "Preserves the formatting of the text during translation")]
    public bool? PreserveFormatting { get; set; }

    [Display("Outline detection", Description = "Detects and preserves document outlines during translation")]
    public bool? OutlineDetection { get; set; }

    [Display("Non-splitting tags", Description = "Tags that should not be split during translation")]
    public List<string>? NonSplittingTags { get; set; }

    [Display("Splitting tags", Description = "Tags that can be split during translation")]
    public List<string>? SplittingTags { get; set; }

    [Display("Ignore tags", Description = "Tags that should be ignored during translation")]
    public List<string>? IgnoreTags { get; set; }

    [Display("Context")]
    public string? Context { get; set; }

    public static TextTranslationRequest FromXliffUnit(TranslationUnit translationUnit,
        XliffTranslationRequest request)
    {
        return new TextTranslationRequest
        {
            Text = translationUnit.Source!,
            SourceLanguage = request.SourceLanguage,
            TargetLanguage = request.TargetLanguage,
            Formality = request.Formality,
            GlossaryId = request.GlossaryId,
            ModelType = request.ModelType,
            PreserveFormatting = request.PreserveFormatting,
            OutlineDetection = request.OutlineDetection,
            NonSplittingTags = request.NonSplittingTags,
            SplittingTags = request.SplittingTags,
            IgnoreTags = request.IgnoreTags,
            Context = request.Context
        };
    }
    
    public static TextTranslationRequest CreateBatchRequest(IEnumerable<string> texts, 
        XliffTranslationRequest request)
    {
        return new TextTranslationRequest
        {
            Text = string.Join("\n", texts),
            TargetLanguage = request.TargetLanguage,
            SourceLanguage = request.SourceLanguage,
            Formality = request.Formality,
            GlossaryId = request.GlossaryId,
            ModelType = request.ModelType,
            PreserveFormatting = request.PreserveFormatting,
            OutlineDetection = request.OutlineDetection,
            NonSplittingTags = request.NonSplittingTags,
            SplittingTags = request.SplittingTags,
            IgnoreTags = request.IgnoreTags,
            Context = request.Context
        };
    }
}