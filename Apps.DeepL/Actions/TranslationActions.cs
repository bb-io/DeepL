using Apps.DeepL.Requests;
using Apps.DeepL.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using DeepL;

namespace Apps.DeepL.Actions;

[ActionList]
public class TranslationActions : DeepLInvocable
{
    private readonly IFileManagementClient _fileManagementClient;

    public TranslationActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(
        invocationContext)
    {
        _fileManagementClient = fileManagementClient;
    }

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
        var file = await _fileManagementClient.DownloadAsync(request.File);

        var outputStream = new MemoryStream();
        await Client.TranslateDocumentAsync(file, request.File.Name, outputStream, request.SourceLanguage,
            request.TargetLanguage, CreateDocumentTranslateOptions(request));

        var newFileName = request.File.Name;

        if (request.TranslateFileName.HasValue && request.TranslateFileName.Value) 
        {
            var translateResponse = await Translate(new TextTranslationRequest { 
                Formal = request.Formal, 
                GlossaryId = request.GlossaryId, 
                SourceLanguage = request.SourceLanguage,
                TargetLanguage = request.TargetLanguage,
                Text = request.File.Name,
            });
            newFileName = translateResponse.TranslatedText;
        }

        var uploadedFile = await _fileManagementClient.UploadAsync(outputStream, newFileName, request.File.ContentType);
        return new()
        {
            File = uploadedFile
        };
    }

    private TextTranslateOptions CreateTextTranslateOptions(TextTranslationRequest request)
    {
        var options = new TextTranslateOptions
        {
            PreserveFormatting = request.PreserveFormatting == null || (bool)request.PreserveFormatting,
            Formality = request.Formal == null
                ? Formality.Default
                : (bool)request.Formal ? Formality.PreferMore : Formality.PreferLess,
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
        return new DocumentTranslateOptions
        {
            Formality = request.Formal == null
                ? Formality.Default
                : (bool)request.Formal ? Formality.PreferMore : Formality.PreferLess,
            GlossaryId = request.GlossaryId,
        };
    }
}