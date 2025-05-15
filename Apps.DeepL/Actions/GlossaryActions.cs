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
using Apps.DeepL.Utils;
using RestSharp;
using Apps.DeepL.Models;
using System.Xml.Linq;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.DeepL.Actions;

[ActionList]
public class GlossaryActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : DeepLInvocable(invocationContext)
{
    private const string MissingGlossaryLanguageMessage = "Glossary file is missing terms for language: {0}";

    [Action("Export glossary", Description = "Export glossary")]
    public async Task<ExportGlossaryResponse> ExportGlossary([ActionParameter] GlossaryRequest request)
    {
        var glossaryDetails = await ErrorHandler.ExecuteWithErrorHandlingAsync(async () => await Client.GetGlossaryAsync(request.GlossaryId));
        var glossaryEntries = await ErrorHandler.ExecuteWithErrorHandlingAsync(async () => await Client.GetGlossaryEntriesAsync(request.GlossaryId));
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
            File = await fileManagementClient.UploadAsync(stream, MediaTypeNames.Application.Xml,
                $"{glossaryDetails.Name}.tbx")
        };
    }

    [Action("Import glossary", Description = "Import glossary")] //Create glossary v2
    public async Task<NewGlossaryResponse> ImportGlossary([ActionParameter] ImportGlossaryRequest request)
    {
        request.TargetLanguageCode = request.TargetLanguageCode.ToLower();
        request.SourceLanguageCode = request.SourceLanguageCode.ToLower();
        await using var glossaryStream = await fileManagementClient.DownloadAsync(request.File);
        var fileExtension = Path.GetExtension(request.File.Name);

        var (glossaryEntries, glossaryTitle) = fileExtension switch
        {
            ".tbx" => await GetEntriesFromTbx(request, glossaryStream),
            ".csv" => GetEntriesFromCsv(request, glossaryStream),
            ".tsv" => GetEntriesFromTsv(request, glossaryStream),
            _ => throw new Exception($"Glossary format not supported ({fileExtension})." +
                                     "Supported file extensions include .tbx, .csv & .tsv")
        };

        var result = await ErrorHandler.ExecuteWithErrorHandlingAsync(async () => await Client.CreateGlossaryAsync(glossaryTitle,
            request.SourceLanguageCode, request.TargetLanguageCode, glossaryEntries));
        await await ErrorHandler.ExecuteWithErrorHandlingAsync(async () => Client.WaitUntilGlossaryReadyAsync(result.GlossaryId));

        return new NewGlossaryResponse
        {
            GossaryId = result.GlossaryId,
            Name = result.Name,
            SourceLanguageCode = result.SourceLanguageCode,
            TargetLanguageCode = result.TargetLanguageCode,
            EntryCount = result.EntryCount,
        };
    }

    [Action("Import glossary v3", Description = "Import glossary via DeepL v3 endpoint")]
    public async Task<NewGlossaryResponse> ImportGlossaryV3([ActionParameter] ImportGlossaryRequest request)
    {
        await using var stream = await fileManagementClient.DownloadAsync(request.File);
        await using var memStream = new MemoryStream();
        await stream.CopyToAsync(memStream).ConfigureAwait(false);
        var fileBytes = memStream.ToArray();

        var ext = Path.GetExtension(request.File.Name).ToLowerInvariant();
        string glossaryName;
        var dictionariesPayload = new List<object>();

        if (ext == ".tbx")
        {
            var xdoc = XDocument.Load(new MemoryStream(fileBytes));
            XNamespace ns = xdoc.Root.GetDefaultNamespace();

            glossaryName = string.IsNullOrWhiteSpace(request.Name)
                ? xdoc.Descendants(ns + "title").FirstOrDefault()?.Value
                  ?? Path.GetFileNameWithoutExtension(request.File.Name)
                : request.Name;

            var langs = xdoc
                .Descendants(ns + "langSec")
                .Attributes(XNamespace.Xml + "lang")
                .Select(a => a.Value.ToLower())
                .Distinct()
                .ToList();

            var pivotLang = xdoc.Root.Attribute(XNamespace.Xml + "lang")?.Value.ToLower()
                            ?? langs.First();

            foreach (var targetLang in langs.Where(l => l != pivotLang))
            {
                var reqPair = new ImportGlossaryRequest
                {
                    File = request.File,
                    SourceLanguageCode = pivotLang,
                    TargetLanguageCode = targetLang,
                    Name = request.Name
                };
                await using var pairStream = new MemoryStream(fileBytes);
                var (entriesPair, _) = await GetEntriesFromTbx(reqPair, pairStream);
                var tsvPair = entriesPair.ToTsv();

                dictionariesPayload.Add(new
                {
                    source_lang = pivotLang,
                    target_lang = targetLang,
                    entries = tsvPair,
                    entries_format = "tsv"
                });
            }
        }
        else
        {
            await using var cueStream = new MemoryStream(fileBytes);
            (GlossaryEntries entries, string name) data;
            string format;
            switch (ext)
            {
                case ".csv":
                    data = GetEntriesFromCsv(request, cueStream);
                    format = "csv";
                    break;
                case ".tsv":
                    data = GetEntriesFromTsv(request, cueStream);
                    format = "tsv";
                    break;
                default:
                    throw new PluginMisconfigurationException($"Unsupported format: {ext}");
            }
            glossaryName = string.IsNullOrWhiteSpace(request.Name) ? data.name : request.Name;
            var tsv = data.entries.ToTsv();
            var entriesText = format == "tsv" ? tsv : tsv.Replace("	", ",");

            dictionariesPayload.Add(new
            {
                source_lang = request.SourceLanguageCode.ToLower(),
                target_lang = request.TargetLanguageCode.ToLower(),
                entries = entriesText,
                entries_format = format
            });
        }

        var body = new
        {
            name = glossaryName,
            dictionaries = dictionariesPayload.ToArray()
        };

        var restReq = new RestRequest("https://api.deepl.com/v3/glossaries", Method.Post)
            .AddJsonBody(body);

        var resp = await RestClient.ExecuteAsync<CreateGlossaryV3Result>(restReq).ConfigureAwait(false);
        if (!resp.IsSuccessful)
            throw new PluginApplicationException($"DeepL v3 import error: {resp.StatusCode} – {resp.Content}");

        var result = resp.Data!;

        var info = result.Dictionaries.First();
        return new NewGlossaryResponse
        {
            GossaryId = result.GlossaryId,
            Name = result.Name,
            SourceLanguageCode = info.SourceLang,
            TargetLanguageCode = info.TargetLang,
            EntryCount = info.EntryCount
        };
    }

    [Action("Update dictionary v3", Description = "Add a new dictionary or replace an existing one via DeepL v3 endpoint")]
    public async Task<NewGlossaryResponse> AddOrReplaceDictionaryV3([ActionParameter] UpdateGlossaryRequest request)
    {
        await using var stream = await fileManagementClient.DownloadAsync(request.File);
        await using var memStream = new MemoryStream();
        await stream.CopyToAsync(memStream).ConfigureAwait(false);
        var fileBytes = memStream.ToArray();

        var ext = Path.GetExtension(request.File.Name).ToLowerInvariant();
        string lastSource = null, lastTarget = null;
        int lastCount = 0;
        string format;

        if (ext == ".tbx")
        {
            var xdoc = XDocument.Load(new MemoryStream(fileBytes));
            XNamespace ns = xdoc.Root.GetDefaultNamespace();
            var pivotLang = xdoc.Root.Attribute(XNamespace.Xml + "lang")?.Value.ToLower()
                            ?? xdoc.Descendants(ns + "langSec").Attributes(XNamespace.Xml + "lang").First().Value.ToLower();
            var langs = xdoc.Descendants(ns + "langSec").Attributes(XNamespace.Xml + "lang").Select(a => a.Value.ToLower()).Distinct();

            foreach (var targetLang in langs.Where(l => l != pivotLang))
            {
                var reqPair = new ImportGlossaryRequest
                {
                    File = request.File,
                    SourceLanguageCode = pivotLang,
                    TargetLanguageCode = targetLang
                };
                await using var pairStream = new MemoryStream(fileBytes);
                var (entriesPair, _) = await GetEntriesFromTbx(reqPair, pairStream);
                var tsvPair = entriesPair.ToTsv();

                var body = new
                {
                    source_lang = pivotLang,
                    target_lang = targetLang,
                    entries = tsvPair,
                    entries_format = "tsv"
                };
                var restReq = new RestRequest($"https://api.deepl.com/v3/glossaries/{request.GlossaryId}/dictionaries", Method.Put)
                    .AddJsonBody(body);
                var resp = await RestClient.ExecuteAsync<AddOrReplaceDictionaryResult>(restReq).ConfigureAwait(false);
                if (!resp.IsSuccessful)
                    throw new PluginApplicationException($"DeepL v3 add/replace dictionary error: {resp.StatusCode} – {resp.Content}");
                var data = resp.Data!;
                lastSource = data.SourceLang;
                lastTarget = data.TargetLang;
                lastCount = data.EntryCount;
            }
        }
        else
        {
            await using var cueStream = new MemoryStream(fileBytes);
            (GlossaryEntries entries, string name) data;
            switch (ext)
            {
                case ".csv":
                    data = GetEntriesFromCsv(request, cueStream);
                    format = "csv";
                    break;
                case ".tsv":
                    data = GetEntriesFromTsv(request, cueStream);
                    format = "tsv";
                    break;
                default:
                    throw new NotSupportedException($"Unsupported format: {ext}");
            }
            var tsv = data.entries.ToTsv();
            var entriesText = format == "tsv" ? tsv : tsv.Replace("	", ",");
            var body = new
            {
                source_lang = request.SourceLanguageCode.ToLower(),
                target_lang = request.TargetLanguageCode.ToLower(),
                entries = entriesText,
                entries_format = format
            };
            var restReq = new RestRequest($"https://api.deepl.com/v3/glossaries/{request.GlossaryId}/dictionaries", Method.Put)
                .AddJsonBody(body);
            var resp = await RestClient.ExecuteAsync<AddOrReplaceDictionaryResult>(restReq).ConfigureAwait(false);
            if (!resp.IsSuccessful)
                throw new PluginApplicationException($"DeepL v3 add/replace dictionary error: {resp.StatusCode} – {resp.Content}");
            var dataRes = resp.Data!;
            lastSource = dataRes.SourceLang;
            lastTarget = dataRes.TargetLang;
            lastCount = dataRes.EntryCount;
        }

        return new NewGlossaryResponse
        {
            GossaryId = request.GlossaryId,
            Name = request.GlossaryId,
            SourceLanguageCode = lastSource,
            TargetLanguageCode = lastTarget,
            EntryCount = lastCount
        };
    }

    [Action("Export glossary v3", Description = "Export glossary via DeepL v3 endpoint")]
    public async Task<ExportGlossaryResponse> ExportGlossaryV3([ActionParameter] GlossaryRequest request)
    {
        var metaReq = new RestRequest($"https://api.deepl.com/v3/glossaries/{request.GlossaryId}", Method.Get);
        var metaResp = await RestClient.ExecuteAsync<GetGlossaryMetadataResult>(metaReq).ConfigureAwait(false);
        if (!metaResp.IsSuccessful)
            throw new Exception($"DeepL v3 metadata error: {metaResp.StatusCode} – {metaResp.Content}");
        var metadata = metaResp.Data!;

        var pivotLang = metadata.Dictionaries.First().SourceLang;

        var termMap = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var dictInfo in metadata.Dictionaries)
        {
            var entriesReq = new RestRequest(
                $"https://api.deepl.com/v3/glossaries/{request.GlossaryId}/entries?source_lang={pivotLang}&target_lang={dictInfo.TargetLang}",
                Method.Get);
            var entriesResp = await RestClient.ExecuteAsync<GetGlossaryEntriesResult>(entriesReq).ConfigureAwait(false);
            if (!entriesResp.IsSuccessful)
                throw new PluginApplicationException($"DeepL v3 entries error: {entriesResp.StatusCode} – {entriesResp.Content}");
            var dictData = entriesResp.Data!.Dictionaries.First();
            var entries = GlossaryEntries.FromTsv(dictData.Entries, skipChecks: true).ToDictionary();

            foreach (var kvp in entries)
            {
                if (!termMap.ContainsKey(kvp.Key))
                    termMap[kvp.Key] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        [pivotLang] = kvp.Key
                    };
                termMap[kvp.Key][dictInfo.TargetLang] = kvp.Value;
            }
        }

        var conceptEntries = new List<GlossaryConceptEntry>();
        int id = 0;
        foreach (var kv in termMap)
        {
            var sections = kv.Value
                .Select(pair => new GlossaryLanguageSection(pair.Key,
                    new List<GlossaryTermSection> { new GlossaryTermSection(pair.Value) }))
                .ToList();
            conceptEntries.Add(new GlossaryConceptEntry((id++).ToString(), sections));
        }

        var blackbirdGlossary = new Glossary(conceptEntries)
        {
            Title = metadata.Name
        };
        await using var tbxStream = blackbirdGlossary.ConvertToTBX();
        return new ExportGlossaryResponse
        {
            File = await fileManagementClient.UploadAsync(
                tbxStream,
                MediaTypeNames.Application.Xml,
                $"{metadata.Name}.tbx")
        };
    }



    [Action("List glossaries", Description = "List all glossaries")]
    public async Task<ListGlossariesResponse> ListGlossaries()
    {
        var glossary = await ErrorHandler.ExecuteWithErrorHandlingAsync(async () => await Client.ListGlossariesAsync());
        return new(glossary.Select(x => new GlossaryEntity(x)));
    }

    [Action("Get glossary", Description = "Get details of a specific glossary")]
    public async Task<GlossaryEntity> GetGlossary([ActionParameter] GlossaryRequest input)
    {
        var glossary = await ErrorHandler.ExecuteWithErrorHandlingAsync(async () => await Client.GetGlossaryAsync(input.GlossaryId));
        return new(glossary);
    }

    [Action("List glossary language pairs", Description = "List supported glossary language pairs")]
    public async Task<ListLanguagePairsResponse> ListGlossaryLanguagePairs()
    {
        var glossary = await ErrorHandler.ExecuteWithErrorHandlingAsync(async () => await Client.GetGlossaryLanguagesAsync());
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
        var entries = await ErrorHandler.ExecuteWithErrorHandlingAsync(async () => await Client.GetGlossaryEntriesAsync(input.GlossaryId));
        var tsvContent = Encoding.UTF8.GetBytes(entries.ToTsv());

        return new()
        {
            File = await fileManagementClient.UploadAsync(new MemoryStream(tsvContent), "text/tab-separated-values",
                $"{input.GlossaryId}.tsv")
        };
    }
    
    [Action("Delete glossary", Description = "Delete a glossary")]
    public async Task DeleteGlossary([ActionParameter] GlossaryRequest input)
    {
        await ErrorHandler.ExecuteWithErrorHandlingAsync(async () => await Client.DeleteGlossaryAsync(input.GlossaryId));
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