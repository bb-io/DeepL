using Apps.DeepL.Responses;
using Apps.DeepL.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Xliff.Utils.Extensions;
using DeepL;
using Apps.DeepL.Constants;
using Apps.DeepL.Requests.Content;
using Blackbird.Filters.Transformations;
using Blackbird.Filters.Extensions;
using Blackbird.Filters.Enums;
using DeepL.Model;
using Microsoft.Extensions.Options;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Filters.Constants;

namespace Apps.DeepL.Actions;

[ActionList]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : DeepLInvocable(invocationContext)
{
    [BlueprintActionDefinition(BlueprintAction.TranslateFile)]
    [Action("Translate", Description = "Translate file content retrieved from a CMS or file storage. The output can be used in compatible actions.")]
    public async Task<FileResponse> TranslateContent([ActionParameter] ContentTranslationRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.TargetLanguage))
        {
            throw new PluginMisconfigurationException("The target language can not be empty, please fill the 'Target language' field and make sure it has a valid language code");
        }

        var supportedLanguages = LanguageConstants.TargetLanguages.Keys;
        if (!supportedLanguages.Contains(input.TargetLanguage.ToUpperInvariant()))
        {
            throw new PluginMisconfigurationException($"The target language '{input.TargetLanguage}' is not supported. Please select a valid language.");
        }

        if (input.FileTranslationStrategy == "deepl")
        {
            var translationActions = new TranslationActions(InvocationContext, fileManagementClient);
            return await translationActions.TranslateDocument(new Requests.DocumentTranslationRequest
            {
                File = input.File,
                TargetLanguage = input.TargetLanguage,
                TranslateFileName = false,
                Formality = input.Formality,
                GlossaryId = input.GlossaryId,
                SourceLanguage = input.SourceLanguage,
            });
        }

        try
        {
            var stream = await fileManagementClient.DownloadAsync(input.File);
            var content = await Transformation.Parse(stream, input.File.Name);
            return await HandleInteroperableTransformation(content, input);
        }
        catch (Exception e)
        {
            if (e.Message.Contains("This file format is not supported"))
            {
                throw new PluginMisconfigurationException("The file format is not supported by the Blackbird interoperable setting. Try setting the file translation strategy to DeepL native.");
            }
            throw;
        }
    }


    private async Task<FileResponse> HandleInteroperableTransformation(Transformation content, ContentTranslationRequest input)
    {
        content.SourceLanguage ??= input.SourceLanguage;
        content.TargetLanguage ??= input.TargetLanguage.ToLower();

        var options = new TextTranslateOptions
        {
            PreserveFormatting = input.PreserveFormatting.HasValue ? input.PreserveFormatting.Value : true,
            Formality = GetFormality(input.Formality),
            GlossaryId = input.GlossaryId,
            TagHandling = "html",
            Context = input.Context,
            ModelType = GetModelType(input.ModelType)
        };

        async Task<IEnumerable<TextResult>> BatchTranslate(IEnumerable<Segment> batch)
        {
            return await ErrorHandler.ExecuteWithErrorHandlingAsync(async () =>
                    await Client.TranslateTextAsync(batch.Select(x => x.GetSource()), content.SourceLanguage, input.TargetLanguage, options));
        }

        var segmentTranslations = await content
            .GetSegments()
            .Where(x => !x.IsIgnorbale && x.IsInitial)
            .Batch(100).Process(BatchTranslate);

        var sourceLanguages = new List<string>();
        foreach (var (segment, translation) in segmentTranslations)
        {
            segment.SetTarget(translation.Text);
            segment.State = SegmentState.Translated;
            if (!string.IsNullOrEmpty(translation.DetectedSourceLanguageCode))
            {
                sourceLanguages.Add(translation.DetectedSourceLanguageCode.ToLower());
            }
        }

        if (input.OutputFileHandling == "original")
        {
            var targetContent = content.Target();
            return new FileResponse 
            { 
                File = await fileManagementClient.UploadAsync(targetContent.Serialize().ToStream(), targetContent.OriginalMediaType, targetContent.OriginalName) 
            };
        }

        var mostOccuringSourceLanguage = sourceLanguages
                .GroupBy(s => s)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;

        content.SourceLanguage ??= mostOccuringSourceLanguage;
        content.TargetLanguage ??= input.TargetLanguage;

        return new FileResponse 
        { 
            File = await fileManagementClient.UploadAsync(content.Serialize().ToStream(), MediaTypes.Xliff, content.XliffFileName)
        };
    }

    private static Formality GetFormality(string? formalityString)
    {
        return formalityString switch
        {
            "more" => Formality.More,
            "less" => Formality.Less,
            "prefer_more" => Formality.PreferMore,
            "prefer_less" => Formality.PreferLess,
            _ => Formality.Default
        };
    }

    private static ModelType GetModelType(string? modelTypeString)
    {
        return modelTypeString switch
        {
            "quality_optimized" => ModelType.QualityOptimized,
            "prefer_quality_optimized" => ModelType.PreferQualityOptimized,
            _ => ModelType.LatencyOptimized
        };
    }
}
