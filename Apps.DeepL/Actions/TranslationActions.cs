using System.Xml.Linq;
using Apps.DeepL.Constants;
using Apps.DeepL.Requests;
using Apps.DeepL.Requests.Content;
using Apps.DeepL.Responses;
using Apps.DeepL.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Constants;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Extensions;
using Blackbird.Filters.Transformations;
using Blackbird.Xliff.Utils;
using Blackbird.Xliff.Utils.Converters;
using Blackbird.Xliff.Utils.Extensions;
using DeepL;
using DeepL.Model;

namespace Apps.DeepL.Actions;

[ActionList("Translation")]
public class TranslationActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : DeepLInvocable(invocationContext)
{
    [BlueprintActionDefinition(BlueprintAction.TranslateText)]
    [Action("Translate text", Description = "Translate a single simple text string")]
    public async Task<TextResponse> Translate([ActionParameter] TextTranslationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TargetLanguage))
        {
            throw new PluginMisconfigurationException("The target language can not be empty, please fill the 'Target language' field and make sure it has a valid language code");
        }

        if (string.IsNullOrEmpty(request.Text))
        {
            throw new PluginMisconfigurationException("The text can not be empty, please fill the 'Text' field and make sure it has content");
        }

        var supportedLanguages = LanguageConstants.TargetLanguages.Keys;
        if (!supportedLanguages.Contains(request.TargetLanguage.ToUpperInvariant()))
        {
            throw new PluginMisconfigurationException($"The target language '{request.TargetLanguage}' is not supported. Please select a valid language.");
        }

        var result = await ErrorHandler.ExecuteWithErrorHandlingAsync(async () => 
            await Client.TranslateTextAsync(request.Text, request.SourceLanguage, request.TargetLanguage, CreateTextTranslateOptions(request)));
        return new TextResponse
        {
            TranslatedText = result.Text,
            DetectedSourceLanguage = result.DetectedSourceLanguageCode
        };
    }

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
            return await translationActions.HandlerNativeTranslateDocument(new DocumentTranslationRequest
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

    private async Task<FileResponse> HandlerNativeTranslateDocument([ActionParameter] DocumentTranslationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TargetLanguage))
        {
            throw new PluginMisconfigurationException("The target language can not be empty, please fill the 'Target language' field and make sure it has a valid language code");
        }
        
        var tuple = await GetFileAndXliffDocumentAsync(request);
        var file = tuple.Item1;
        var xliffDocument = tuple.Item2;
        
        using var outputStream = new MemoryStream();

        await ErrorHandler.ExecuteWithErrorHandlingAsync(async () => await Client.TranslateDocumentAsync(file,
            request.File.Name, outputStream, request.SourceLanguage,
            request.TargetLanguage, CreateDocumentTranslateOptions(request)));

        var newFileName = request.File.Name;

        if (request.TranslateFileName.HasValue && request.TranslateFileName.Value)
        {
            var translateResponse = await Translate(new TextTranslationRequest
            {
                Formality = request.Formality,
                GlossaryId = request.GlossaryId,
                SourceLanguage = request.SourceLanguage,
                TargetLanguage = request.TargetLanguage,
                Text = request.File.Name,
            });
            newFileName = translateResponse.TranslatedText;
        }
        
        using var memoryStream = new MemoryStream(outputStream.GetBuffer());
        memoryStream.Position = 0;

        XDocument? result = null;
        if (xliffDocument != null)
        {
            if (tuple.Item2?.Version == "1.2")
            {
                var xliff12 = Xliff21To12Converter.Convert(memoryStream.ToXliffDocument());
                var finalFileStream = xliff12.ToStream();
                var uploadedFinalFile = await fileManagementClient.UploadAsync(finalFileStream, request.File.ContentType, newFileName);
                return new FileResponse { File = uploadedFinalFile };
            }
            
            var xliffDocument21 = memoryStream.ToXliffDocument();
            result = xliffDocument?.UpdateTranslationUnits(xliffDocument21.TranslationUnits);
        }

        await using var outputFileStream = result?.ToStream() ?? memoryStream;
        var uploadedFile = await fileManagementClient.UploadAsync(outputFileStream, request.File.ContentType, newFileName);
        return new FileResponse { File = uploadedFile };
    }
    
    private TextTranslateOptions CreateTextTranslateOptions(TextTranslationRequest request)
    {
        var options = new TextTranslateOptions
        {
            PreserveFormatting = request.PreserveFormatting == null || (bool)request.PreserveFormatting,
            Formality = GetFormality(request.Formality),
            GlossaryId = request.GlossaryId,
            TagHandling = request.TagHandling,
            OutlineDetection = request.OutlineDetection == null || (bool)request.OutlineDetection,
            Context = request.Context,
            ModelType = GetModelType(request.ModelType)
        };

        AddTagsToOptions(options, request);
        return options;
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

    private static void AddTagsToOptions(TextTranslateOptions options, TextTranslationRequest request)
    {
        if (request.NonSplittingTags != null)
            options.NonSplittingTags.AddRange(request.NonSplittingTags);

        if (request.SplittingTags != null)
            options.SplittingTags.AddRange(request.SplittingTags);

        if (request.IgnoreTags != null)
            options.IgnoreTags.AddRange(request.IgnoreTags);
    }

    private DocumentTranslateOptions CreateDocumentTranslateOptions(DocumentTranslationRequest request)
    {
        var formality = Formality.Default;

        switch (request.Formality)
        {
            case "more": formality = Formality.More; break;
            case "less": formality = Formality.Less; break;
            case "prefer_more": formality = Formality.PreferMore; break;
            case "prefer_less": formality = Formality.PreferLess; break;
            default:
                break;
        }

        return new DocumentTranslateOptions
        {
            Formality = formality,
            GlossaryId = request.GlossaryId,
        };
    }

    private async Task<(Stream, XliffDocument?)> GetFileAndXliffDocumentAsync(DocumentTranslationRequest request)
    {
        if (request.File == null)
        {
            throw new PluginMisconfigurationException("No file provided. Please provide a valid file and try again.");
        }

        var fileStream = await fileManagementClient.DownloadAsync(request.File);

        var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        if (memoryStream.ReadByte() == -1)
        {
            throw new PluginMisconfigurationException("The provided file is empty. Please provide a valid file and try again.");
        }
        memoryStream.Position = 0;

        if (request.File.Name.EndsWith(".xliff") || request.File.Name.EndsWith(".xlf"))
        {
            var xliffDoc = XDocument.Load(memoryStream);
            memoryStream.Position = 0;

            var version = xliffDoc.GetVersion();

            if(version != "1.2" || version != "2.1")
            {
                throw new PluginMisconfigurationException($"The version '{version}' of the provided XLIFF file is not supported. Please provide a valid XLIFF file with version 1.2 or 2.1.");
            }

            if (version == "1.2")
            {
                XliffDocument xliffDocument = memoryStream.ToXliffDocument();

                var converted = Xliff12To21Converter.Convert(xliffDocument);
                return (converted.ToStream(), xliffDocument);
            }
        }

        return (memoryStream, null);
    }
}