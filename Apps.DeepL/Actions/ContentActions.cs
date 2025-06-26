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

namespace Apps.DeepL.Actions;

[ActionList]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : DeepLInvocable(invocationContext)
{
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

        var stream = await fileManagementClient.DownloadAsync(input.File);
        try
        {
            var content = await Transformation.Parse(stream);
            return await HandleInteroperableTransformation(content, input);
        }
        catch (Exception)
        {
            var translationActions = new TranslationActions(InvocationContext, fileManagementClient);
            return await translationActions.TranslateDocument(new Requests.DocumentTranslationRequest { 
                File = input.File,  
                TargetLanguage = input.TargetLanguage,
                TranslateFileName = false,
                Formality = input.Formality,
                GlossaryId = input.GlossaryId,
                SourceLanguage = input.SourceLanguage,
            });
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

        var batches = content.GetSegments().Where(x => !x.IsIgnorbale && x.IsInitial).Batch(100);

        var sourceLanguages = new List<string>();

        foreach (var batch in batches)
        {
            var results = await ErrorHandler.ExecuteWithErrorHandlingAsync(async () =>
                await Client.TranslateTextAsync(batch.Select(x => x.GetSource()), content.SourceLanguage, input.TargetLanguage, options));

            var batchAsArray = batch.ToArray();
            for (int i = 0; i < results.Length; i++)
            {
                var segment = batchAsArray[i];
                var result = results[i];

                segment.SetTarget(result.Text);
                segment.State = SegmentState.Translated;
                if (!string.IsNullOrEmpty(result.DetectedSourceLanguageCode))
                {
                    sourceLanguages.Add(result.DetectedSourceLanguageCode.ToLower());
                }
            }
        }

        if (input.OutputFileHandling == null || input.OutputFileHandling == "xliff")
        {
            var mostOccuringSourceLanguage = sourceLanguages
            .GroupBy(s => s)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;

            content.SourceLanguage ??= mostOccuringSourceLanguage;

            var xliffStream = content.Serialize().ToStream();
            var fileName = input.File.Name.EndsWith("xliff") || input.File.Name.EndsWith("xlf") ? input.File.Name : input.File.Name + ".xliff";
            var uploadedFile = await fileManagementClient.UploadAsync(xliffStream, "application/xliff+xml", fileName);
            return new FileResponse { File = uploadedFile };
        }
        else
        {
            var resultStream = content.Target().Serialize().ToStream();
            var uploadedFile = await fileManagementClient.UploadAsync(resultStream, "application/xliff+xml", input.File.Name);
            return new FileResponse { File = uploadedFile };
        }
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
