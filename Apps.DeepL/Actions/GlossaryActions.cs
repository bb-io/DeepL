using Apps.DeepL.Requests;
using Apps.DeepL.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Glossaries.Utils.Converters;
using Blackbird.Applications.Sdk.Glossaries.Utils.Models.Dtos;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using DeepL;
using System.Net.Mime;
using System.Text;
using Apps.DeepL.Entities;
using Apps.DeepL.Responses.Glossaries;

namespace Apps.DeepL.Actions;

[ActionList]
public class GlossaryActions : DeepLInvocable
{
    private readonly IFileManagementClient _fileManagementClient;

    private const string MissingGlossaryLanguageMessage = "Glossary file is missing terms for language: {0}";

    public GlossaryActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(
        invocationContext)
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
            var glossaryTermSection1 = new List<GlossaryTermSection> { new(entry.Key) };

            var glossaryTermSection2 = new List<GlossaryTermSection> { new(entry.Value) };

            var languageSections = new List<GlossaryLanguageSection>
            {
                new(glossaryDetails.SourceLanguageCode, glossaryTermSection1),
                new(glossaryDetails.TargetLanguageCode, glossaryTermSection2)
            };

            conceptEntries.Add(new GlossaryConceptEntry(counter.ToString(), languageSections));
            ++counter;
        }

        var blackbirdGlossary = new Glossary(conceptEntries)
        {
            Title = glossaryDetails.Name
        };
        await using var stream = blackbirdGlossary.ConvertToTBX();
        return new ExportGlossaryResponse()
        {
            File = await _fileManagementClient.UploadAsync(stream, MediaTypeNames.Application.Xml,
                $"{glossaryDetails.Name}.tbx")
        };
    }

    [Action("Import glossary", Description = "Import glossary")]
    public async Task<NewGlossaryResponse> ImportGlossary([ActionParameter] ImportGlossaryRequest request)
    {
        request.TargetLanguageCode = request.TargetLanguageCode.ToLower();
        request.SourceLanguageCode = request.SourceLanguageCode.ToLower();
        await using var glossaryStream = await _fileManagementClient.DownloadAsync(request.File);
        var fileExtension = Path.GetExtension(request.File.Name);

        var (glossaryEntries, glossaryTitle) = fileExtension switch
        {
            ".tbx" => await GetEntriesFromTbx(request, glossaryStream),
            ".csv" => GetEntriesFromCsv(request, glossaryStream),
            ".tsv" => GetEntriesFromTsv(request, glossaryStream),
            _ => throw new Exception($"Glossary format not supported ({fileExtension})." +
                                     "Supported file extensions include .tbx, .csv & .tsv")
        };

        var result = await Client.CreateGlossaryAsync(glossaryTitle,
            request.SourceLanguageCode, request.TargetLanguageCode, glossaryEntries);
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

    [Action("List glossaries", Description = "List all glossaries")]
    public async Task<ListGlossariesResponse> ListGlossaries()
    {
        var glossary = await Client.ListGlossariesAsync();
        return new(glossary.Select(x => new GlossaryEntity(x)));
    }

    [Action("Get glossary", Description = "Get details of a specific glossary")]
    public async Task<GlossaryEntity> GetGlossary([ActionParameter] GlossaryRequest input)
    {
        var glossary = await Client.GetGlossaryAsync(input.GlossaryId);
        return new(glossary);
    }

    [Action("List glossary language pairs", Description = "List supported glossary language pairs")]
    public async Task<ListLanguagePairsResponse> ListGlossaryLanguagePairs()
    {
        var glossary = await Client.GetGlossaryLanguagesAsync();
        return new()
        {
            LanguagePairs = glossary.Select(x => new LanguagePairEntity()
            {
                SourceLanguage = x.SourceLanguageCode,
                TargetLanguage = x.TargetLanguageCode
            })
        };
    }

    [Action("Get glossary entries", Description = "Get glossary entries in a TSV format")]
    public async Task<FileResponse> GetGlossaryEntries([ActionParameter] GlossaryRequest input)
    {
        var entries = await Client.GetGlossaryEntriesAsync(input.GlossaryId);
        var tsvContent = Encoding.UTF8.GetBytes(entries.ToTsv());

        return new()
        {
            File = await _fileManagementClient.UploadAsync(new MemoryStream(tsvContent), "text/tab-separated-values",
                $"{input.GlossaryId}.tsv")
        };
    }
    
    [Action("Delete glossary", Description = "Delete a glossary")]
    public async Task DeleteGlossary([ActionParameter] GlossaryRequest input)
    {
        await Client.DeleteGlossaryAsync(input.GlossaryId);
    }

    private string CleanText(string input)
    {
        return input.Replace("\r", "").Replace("\n", " ").Replace("\u2028", "");
    }

    private async Task<(GlossaryEntries entries, string name)> GetEntriesFromTbx(ImportGlossaryRequest request,
        Stream glossaryStream)
    {
        var blackbirdGlossary = await glossaryStream.ConvertFromTBX();
        var glossaryValues = new List<KeyValuePair<string, string>>();
        foreach (var entry in blackbirdGlossary.ConceptEntries)
        {
            var langSectionSource =
                entry.LanguageSections.FirstOrDefault(x => x.LanguageCode.ToLower() == request.SourceLanguageCode);
            if (langSectionSource is null && request.SourceLanguageCode == "en") 
            {
                langSectionSource =
                entry.LanguageSections.FirstOrDefault(x => x.LanguageCode.ToLower() == "en-us" || x.LanguageCode.ToLower() == "en-gb");
            }
            var langSectionTarget =
                entry.LanguageSections.FirstOrDefault(x => x.LanguageCode.ToLower() == request.TargetLanguageCode);
            if (langSectionTarget is null)
            {
                langSectionTarget =
                entry.LanguageSections.FirstOrDefault(x => x.LanguageCode.ToLower().Substring(0, 2) == request.TargetLanguageCode.Substring(0, 2));
            }
            if (langSectionSource == null || langSectionTarget == null)
            {
                continue;
            }

            var cleanTermSource = CleanText(langSectionSource.Terms.First().Term);
            var cleanTermTarget = CleanText(langSectionTarget.Terms.First().Term);
            glossaryValues.Add(new KeyValuePair<string, string>(cleanTermSource, cleanTermTarget));
        }

        return (new GlossaryEntries(glossaryValues.DistinctBy(x => x.Key), skipChecks: true),
            request.Name ?? blackbirdGlossary.Title!);
    }

    private (GlossaryEntries entries, string name) GetEntriesFromCsv(ImportGlossaryRequest request,
        Stream glossaryStream)
    {
        var lines = new List<string>();
        using (StreamReader reader = new StreamReader(glossaryStream))
        {
            while (!reader.EndOfStream)
            {
                lines.Add(reader.ReadLine()!);
            }
        }

        var entries = new List<KeyValuePair<string, string>>();
        foreach (var line in lines)
        {
            entries.Add(new KeyValuePair<string, string>(line.Split(',')[0], line.Split(',')[1]));
        }

        return (new GlossaryEntries(entries.DistinctBy(x => x.Key)), request.Name ?? request.File.Name);
    }

    private (GlossaryEntries entries, string name) GetEntriesFromTsv(ImportGlossaryRequest request,
        Stream glossaryStream)
    {
        var tsvLines = new List<string>();
        using (StreamReader reader = new StreamReader(glossaryStream))
        {
            while (!reader.EndOfStream)
            {
                tsvLines.Add(reader.ReadLine()!);
            }
        }

        var tsvEntries = new List<KeyValuePair<string, string>>();
        foreach (var line in tsvLines)
        {
            tsvEntries.Add(new KeyValuePair<string, string>(line.Split('\t')[0], line.Split('\t')[1]));
        }

        return (new GlossaryEntries(tsvEntries.DistinctBy(x => x.Key)), request.Name ?? request.File.Name);
    }
}