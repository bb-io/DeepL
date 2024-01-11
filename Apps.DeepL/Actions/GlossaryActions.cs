using Apps.DeepL.Requests;
using Apps.DeepL.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Glossaries.Utils.Converters;
using Blackbird.Applications.Sdk.Glossaries.Utils.Models.Dtos;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using DeepL;
using System.IO;
using System.Net.Mime;

namespace Apps.DeepL.Actions;

[ActionList]
public class GlossaryActions : DeepLInvocable
{
    private readonly IFileManagementClient _fileManagementClient;
    public GlossaryActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext) 
    {
        _fileManagementClient = fileManagementClient;
    }

    //[Action("Create glossary", Description = "Create a new glossary")]
    //public async Task<NewGlossaryResponse> CreateGlossary([ActionParameter] CreateGlossaryRequest request)
    //{
    //    if (request.File.ContentType != "text/csv")
    //        throw new Exception("The file type should be text/csv");

    //    var dict = new Dictionary<string, string> {  };
    //    var result = await Client.CreateGlossaryAsync(request.Name, request.SourceLanguage, request.TargetLanguage, new GlossaryEntries(dict));
    //    await Client.WaitUntilGlossaryReadyAsync(result.GlossaryId);
    //    return new NewGlossaryResponse
    //    {
    //        GossaryId = result.GlossaryId,
    //        Name = result.Name,
    //        SourceLanguageCode = result.SourceLanguageCode,
    //        TargetLanguageCode = result.TargetLanguageCode,
    //        EntryCount = result.EntryCount,
    //    };
    //}

    //[Action("Download glossary", Description = "Download a specified glossary for use in other apps")]
    //public async Task<DownloadGlossaryResponse> DownloadGlossary([ActionParameter] GlossaryRequest request)
    //{
    //    var result = await Client.GetGlossaryEntriesAsync(request.GlossaryId);
    //    return new DownloadGlossaryResponse
    //    {
    //    };
    //}

    //[Action("Delete glossary", Description = "Delete a glossary")]
    //public async Task DeleteGlossary([ActionParameter] GlossaryRequest request)
    //{
    //    await Client.DeleteGlossaryAsync(request.GlossaryId);
    //}

    [Action("Export glossary", Description = "Export glossary")]
    public async Task<ExportGlossaryResponse> ExportGlossary([ActionParameter] GlossaryRequest request)
    {
        var glossaryDetails = await Client.GetGlossaryAsync(request.GlossaryId);
        var glossaryEntries = await Client.GetGlossaryEntriesAsync(request.GlossaryId);
        var entries = glossaryEntries.ToDictionary();

        var conceptEntries = new List<GlossaryConceptEntry>();
        int counter = 0;
        foreach (var entry in entries)
        {
            var glossaryTermSection1 = new List<GlossaryTermSection>();
            glossaryTermSection1.Add(new GlossaryTermSection(entry.Key));

            var glossaryTermSection2 = new List<GlossaryTermSection>();
            glossaryTermSection2.Add(new GlossaryTermSection(entry.Value));

            var languageSections = new List<GlossaryLanguageSection>();
            languageSections.Add(new GlossaryLanguageSection(glossaryDetails.SourceLanguageCode, glossaryTermSection1));
            languageSections.Add(new GlossaryLanguageSection(glossaryDetails.TargetLanguageCode, glossaryTermSection2));

            conceptEntries.Add(new GlossaryConceptEntry(counter.ToString(), languageSections));
            ++counter;
        }
        var blackbirdGlossary = new Glossary(conceptEntries);
        blackbirdGlossary.Title = glossaryDetails.Name;
        using var stream = blackbirdGlossary.ConvertToTBX();
        return new ExportGlossaryResponse() { File = await _fileManagementClient.UploadAsync(stream, MediaTypeNames.Application.Xml, $"{glossaryDetails.Name}.tbx") };
    }

    [Action("Import glossary", Description = "Import glossary")]
    public async Task ImportGlossary([ActionParameter] ImportGlossaryRequest request)
    {
        using var glossaryStream = await _fileManagementClient.DownloadAsync(request.File);
        var blackbirdGlossary = await glossaryStream.ConvertFromTBX();

        var glosseryValues = new List<KeyValuePair<string, string>>();
        foreach(var entry in blackbirdGlossary.ConceptEntries)
        {
            var langSection1 = entry.LanguageSections.ElementAt(0);
            var langSection2 = entry.LanguageSections.ElementAt(1);
            glosseryValues.Add(new(langSection1.Terms.First().Term, langSection2.Terms.First().Term));
        }
        var glossaryEntries = new GlossaryEntries(glosseryValues);

        var sourceLanguage = blackbirdGlossary.ConceptEntries.First().LanguageSections.ElementAt(0).LanguageCode;
        var targetLanguage = blackbirdGlossary.ConceptEntries.First().LanguageSections.ElementAt(1).LanguageCode;

        await Client.CreateGlossaryAsync(request.Name ?? blackbirdGlossary.Title, sourceLanguage, targetLanguage, glossaryEntries);
    }
}