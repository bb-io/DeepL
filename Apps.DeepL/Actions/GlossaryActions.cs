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

    private readonly string missinGlossaryLanguageMessage = "Glossary file is missing terms for language: {0}";
    
        public GlossaryActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext) 
    {
        _fileManagementClient = fileManagementClient;
    }

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
    public async Task<NewGlossaryResponse> ImportGlossary([ActionParameter] ImportGlossaryRequest request)
    {
        using var glossaryStream = await _fileManagementClient.DownloadAsync(request.File);
        var blackbirdGlossary = await glossaryStream.ConvertFromTBX();

        var glosseryValues = new List<KeyValuePair<string, string>>();
        foreach(var entry in blackbirdGlossary.ConceptEntries)
        {
            var langSectionSource = entry.LanguageSections.FirstOrDefault(x => x.LanguageCode == request.SourceLanguageCode);
            var langSectionTarget = entry.LanguageSections.FirstOrDefault(x => x.LanguageCode == request.TargetLanguageCode);
            if(langSectionSource == null || langSectionTarget == null)
            {
                throw new ArgumentException(langSectionSource == null ? 
                    String.Format(missinGlossaryLanguageMessage, request.SourceLanguageCode) :
                    String.Format(missinGlossaryLanguageMessage, request.TargetLanguageCode));
            }
            glosseryValues.Add(new(langSectionSource.Terms.First().Term, langSectionTarget.Terms.First().Term));
        }
        var glossaryEntries = new GlossaryEntries(glosseryValues);

        var result = await Client.CreateGlossaryAsync(request.Name ?? blackbirdGlossary.Title, request.SourceLanguageCode, request.TargetLanguageCode, glossaryEntries);
        await Client.WaitUntilGlossaryReadyAsync(result.GlossaryId);
        return new NewGlossaryResponse
        {
            GossaryId = result.GlossaryId,
            Name = result.Name,
            SourceLanguageCode = result.SourceLanguageCode,
            TargetLanguageCode = result.TargetLanguageCode,
            EntryCount = result.EntryCount,
        };
    }
}