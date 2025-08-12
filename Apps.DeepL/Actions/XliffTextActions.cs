using Apps.DeepL.Requests;
using Apps.DeepL.Requests.Xliff;
using Apps.DeepL.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Xliff.Utils;
using Blackbird.Xliff.Utils.Extensions;
using Blackbird.Xliff.Utils.Models;

namespace Apps.DeepL.Actions;

[ActionList("Deprecated")]
public class XliffTextActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : DeepLInvocable(invocationContext)
{
    private static readonly string[] SupportedFileExtensions = { ".xliff", ".xlf", ".mqxliff", ".mxliff", ".txlf" };

    [Action("Translate XLIFF", Description = "Translate an XLIFF file using the text translation endpoint. Useful when using the next-generation model for small XLIFF files.")]
    public async Task<FileResponse> TranslateXliff([ActionParameter] XliffTranslationRequest request)
    {
        if(string.IsNullOrEmpty(request.TargetLanguage))
        {
            throw new PluginMisconfigurationException("Target language is null or empty. Please provide a valid target language.");
        }

        if(!SupportedFileExtensions.Contains(Path.GetExtension(request.File.Name)))
        {
            throw new PluginMisconfigurationException("This action only supports XLIFF files and it seems that the file you provided is not an XLIFF file." +
                $"Please provide a valid XLIFF file. Supported file extensions are: {string.Join(", ", SupportedFileExtensions)}");
        }

        var xliffDocument = await LoadXliffDocumentFromFile(request.File);
        await ProcessTranslationUnits(xliffDocument, request);
        return await SaveTranslatedXliffToFile(xliffDocument, request.File);
    }
    
    private async Task<XliffDocument> LoadXliffDocumentFromFile(FileReference file)
    {
        var stream = await fileManagementClient.DownloadAsync(file);
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        
        return memoryStream.ToXliffDocument();
    }
    
    private async Task ProcessTranslationUnits(XliffDocument xliffDocument, XliffTranslationRequest request)
    {
        var translationActions = new TranslationActions(InvocationContext, fileManagementClient);

        var validTranslationUnits = xliffDocument.TranslationUnits
            .Where(unit => !string.IsNullOrEmpty(unit.Source));

        if (request.TranslateOnlyEmptyUnits == true)
        {
            validTranslationUnits = validTranslationUnits
                .Where(unit => string.IsNullOrEmpty(unit.Target));
        }

        if (request.UseBatches == true)
        {
            await ProcessBatchTranslations(validTranslationUnits, request, translationActions);
        }
        else
        {
            await ProcessIndividualTranslations(validTranslationUnits, request, translationActions);
        }
    }
    
    private static async Task ProcessBatchTranslations(
        IEnumerable<TranslationUnit> units,
        XliffTranslationRequest request,
        TranslationActions translationActions)
    {
        const int batchSize = 100;
        
        for (int i = 0; i < units.Count(); i += batchSize)
        {
            var batch = units.Skip(i).Take(batchSize).ToList();
            var batchTexts = batch.Select(unit => unit.Source!).ToList();
            
            var translationRequest = TextTranslationRequest.CreateBatchRequest(batchTexts, request);
            
            var translationResponse = await translationActions.Translate(translationRequest);
            var translatedParts = translationResponse.TranslatedText.Split('\n');
            
            if (translatedParts.Length == batch.Count)
            {
                for (int j = 0; j < batch.Count; j++)
                {
                    var unit = batch[j];
                    unit.Target = translatedParts[j];
                    unit.SourceLanguage = translationResponse.DetectedSourceLanguage;
                }
            }
            else
            {
                await ProcessIndividualTranslations(batch, request, translationActions);
            }
        }
    }
    
    private static async Task ProcessIndividualTranslations(
        IEnumerable<TranslationUnit> units,
        XliffTranslationRequest request,
        TranslationActions translationActions)
    {
        foreach (var unit in units)
        {
            var individualRequest = TextTranslationRequest.FromXliffUnit(unit, request);
            var individualResponse = await translationActions.Translate(individualRequest);
            
            unit.Target = individualResponse.TranslatedText;
            unit.SourceLanguage = individualResponse.DetectedSourceLanguage;
        }
    }
    
    private async Task<FileResponse> SaveTranslatedXliffToFile(XliffDocument xliffDocument, FileReference originalFile)
    {
        var xliffFileStream = xliffDocument.ToStream();
        var fileReference = await fileManagementClient.UploadAsync(
            xliffFileStream, 
            originalFile.ContentType, 
            originalFile.Name);
            
        return new FileResponse { File = fileReference };
    }
}
