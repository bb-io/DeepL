using System.Xml.Linq;
using Apps.DeepL.Requests;
using Apps.DeepL.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Xliff.Utils;
using Blackbird.Xliff.Utils.Converters;
using Blackbird.Xliff.Utils.Extensions;
using DeepL;

namespace Apps.DeepL.Actions;

[ActionList]
public class TranslationActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : DeepLInvocable(invocationContext)
{
    [Action("Translate", Description = "Translate a text")]
    public async Task<TextResponse> Translate([ActionParameter] TextTranslationRequest request)
    {
        var result = await Client.TranslateTextAsync(request.Text, request.SourceLanguage,
            request.TargetLanguage, CreateTextTranslateOptions(request));
        return new TextResponse
        {
            TranslatedText = result.Text,
            DetectedSourceLanguage = result.DetectedSourceLanguageCode
        };
    }

    [Action("Translate document", Description = "Translate a document")]
    public async Task<FileResponse> TranslateDocument([ActionParameter] DocumentTranslationRequest request)
    {
        var tuple = await GetFileAndXliffDocumentAsync(request);
        var file = tuple.Item1;
        var xliffDocument = tuple.Item2;
    
        using var outputStream = new MemoryStream();
        
        await Client.TranslateDocumentAsync(file, request.File.Name, outputStream, request.SourceLanguage,
            request.TargetLanguage, CreateDocumentTranslateOptions(request));

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

        using var outputFileStream = result?.ToStream() ?? memoryStream;
        var uploadedFile = await fileManagementClient.UploadAsync(outputFileStream, request.File.ContentType, newFileName);
        return new FileResponse { File = uploadedFile };
    }
    
    private TextTranslateOptions CreateTextTranslateOptions(TextTranslationRequest request)
    {
        var formality = Formality.Default;

        switch(request.Formality)
        {
            case "more": formality = Formality.More; break;
            case "less": formality = Formality.Less; break;
            case "prefer_more": formality = Formality.PreferMore; break;
            case "prefer_less": formality = Formality.PreferLess; break;
            default:
            break;
        }

        var options = new TextTranslateOptions
        {
            PreserveFormatting = request.PreserveFormatting == null || (bool)request.PreserveFormatting,
            Formality = formality,
            GlossaryId = request.GlossaryId,
            TagHandling = request.TagHandling,
            OutlineDetection = request.OutlineDetection == null || (bool)request.OutlineDetection,
            Context = request.Context,
        };

        if (request.NonSplittingTags != null)
            options.NonSplittingTags.AddRange(request.NonSplittingTags);

        if (request.SplittingTags != null)
            options.SplittingTags.AddRange(request.SplittingTags);

        if (request.IgnoreTags != null)
            options.IgnoreTags.AddRange(request.IgnoreTags);

        return options;
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
        var fileStream = await fileManagementClient.DownloadAsync(request.File);

        var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        if (request.File.Name.EndsWith(".xliff") || request.File.Name.EndsWith(".xlf"))
        {
            var xliffDoc = XDocument.Load(memoryStream);
            memoryStream.Position = 0;

            var version = xliffDoc.GetVersion();
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