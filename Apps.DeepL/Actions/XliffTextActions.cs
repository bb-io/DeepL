using Apps.DeepL.Requests;
using Apps.DeepL.Requests.Xliff;
using Apps.DeepL.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Xliff.Utils;
using Blackbird.Xliff.Utils.Extensions;
using Blackbird.Xliff.Utils.Models;

namespace Apps.DeepL.Actions;

[ActionList]
public class XliffTextActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : DeepLInvocable(invocationContext)
{
    [Action("Translate XLIFF", Description = "Translate an XLIFF file using the text translation endpoint. Useful when using the next-generation model for small XLIFF files.")]
    public async Task<FileResponse> TranslateXliff([ActionParameter] XliffTranslationRequest request)
    {
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
            .Where(unit => !string.IsNullOrEmpty(unit.Source))
            .ToList();
            
        var useBatches = request.UseBatches ?? false;
        if (useBatches)
        {
            await ProcessBatchTranslations(validTranslationUnits, xliffDocument, request, translationActions);
        }
        else
        {
            await ProcessIndividualTranslations(validTranslationUnits, xliffDocument, request, translationActions);
        }
    }
    
    private async Task ProcessBatchTranslations(
        List<TranslationUnit> units, 
        XliffDocument xliffDocument, 
        XliffTranslationRequest request,
        TranslationActions translationActions)
    {
        const int batchSize = 100;
        
        for (int i = 0; i < units.Count; i += batchSize)
        {
            var batch = units.Skip(i).Take(batchSize).ToList();
            var batchTexts = batch.Select(unit => unit.Source!).ToList();
            
            var translationRequest = TextTranslationRequest.CreateBatchRequest(
                batchTexts, 
                xliffDocument.TargetLanguage, 
                xliffDocument.SourceLanguage, 
                request);
            
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
                await ProcessIndividualTranslations(batch, xliffDocument, request, translationActions);
            }
        }
    }
    
    private async Task ProcessIndividualTranslations(
        List<TranslationUnit> units, 
        XliffDocument xliffDocument, 
        XliffTranslationRequest request,
        TranslationActions translationActions)
    {
        foreach (var unit in units)
        {
            var individualRequest = TextTranslationRequest.FromXliffUnit(unit, xliffDocument, request);
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
