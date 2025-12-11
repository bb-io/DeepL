using Apps.DeepL.Constants;
using Apps.DeepL.Entities;
using Apps.DeepL.Models;
using Apps.DeepL.Requests;
using Apps.DeepL.Responses;
using Apps.DeepL.Responses.Glossaries;
using Apps.DeepL.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Glossaries.Utils.Converters;
using Blackbird.Applications.Sdk.Glossaries.Utils.Dtos;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using DeepL;
using RestSharp;
using System.Net.Mime;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml.Linq;

namespace Apps.DeepL.Actions;

[ActionList("Glossaries")]
public class GlossaryActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : DeepLInvocable(invocationContext)
{
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
        await using var stream = blackbirdGlossary.ConvertToTbx();
        return new ExportGlossaryResponse()
        {
            File = await fileManagementClient.UploadAsync(stream, MediaTypeNames.Application.Xml,
                $"{glossaryDetails.Name}.tbx")
        };
    }

    [Action("Import glossary", Description = "Import glossary")] //Create glossary v2
    public async Task<NewGlossaryResponse> ImportGlossary([ActionParameter] ImportGlossaryRequest request)
    {
        if (request == null || request.File == null)
            throw new PluginMisconfigurationException("Request or file cannot be null.");

        return await ErrorHandler.ExecuteWithErrorHandlingAsync(async () =>
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
                _ => throw new PluginMisconfigurationException($"Glossary format not supported ({fileExtension})." +
                                         "Supported file extensions include .tbx, .csv & .tsv")
            };

            var entriesDict = glossaryEntries.ToDictionary();

            if (glossaryEntries == null || entriesDict.Count == 0)
            {
                throw new PluginMisconfigurationException("Glossary file has no entires, please check your input and try again");
            }

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
        });
    }

    [Action("Import glossary (multilingual)", Description = "Import multilingual glossary")]
    public async Task<NewGlossaryResponse> ImportGlossaryV3([ActionParameter] ImportMultilingualGlossaryRequest request)
    {
        if (request == null || request.File == null)
            throw new PluginMisconfigurationException("Request or file cannot be null.");

        return await ErrorHandler.ExecuteWithErrorHandlingAsync(async () =>
        {
            var (fileBytes, ext) = await DownloadGlossaryFile(request.File);

            var (glossaryName, rawPivotLang, fileLangs) = await ParseGlossaryMetadata(
                fileBytes, ext, request.Name, request.File.Name
            );

            var validPivotLang = NormalizeAndValidateLang(rawPivotLang)
                ?? throw new PluginMisconfigurationException(
                        $"The source language '{rawPivotLang}' is not supported by DeepL Glossaries."
                    );

            var (dictionaries, warnings) = await BuildDictionariesPayload(
                fileBytes, ext, rawPivotLang, validPivotLang, fileLangs, request
            );

            var result = await CreateGlossary(glossaryName, dictionaries, warnings);
            var info = result.Dictionaries.FirstOrDefault()
                       ?? new DictionaryInfo { SourceLang = validPivotLang, TargetLang = "mixed", EntryCount = 0 };

            return new NewGlossaryResponse
            {
                GossaryId = result.GlossaryId,
                Name = result.Name,
                SourceLanguageCode = info.SourceLang,
                TargetLanguageCode = info.TargetLang,
                EntryCount = info.EntryCount,
                Warnings = warnings
            };
        });
    }

    [Action("Update dictionary (multilingual)", Description = "Updates multilingual dictionary")]
    public async Task<NewGlossaryResponse> UpdateDictionaryV3([ActionParameter] UpdateGlossaryRequest request)
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
                    throw new PluginApplicationException($"Error: {resp.StatusCode} – {resp.Content}");
                var data = resp.Data!;
                lastSource = data.SourceLang;
                lastTarget = data.TargetLang;
                lastCount = data.EntryCount;
            }
        }
        else if (ext == ".csv" || ext == ".tsv")
        {
            await using var cueStream = new MemoryStream(fileBytes);
            using var reader = new StreamReader(cueStream);

            var headerLine = await reader.ReadLineAsync();
            if (headerLine == null)
                throw new PluginMisconfigurationException("File must contain at least two language codes in header.");

            var separator = ext == ".csv" ? ',' : '\t';
            var langs = headerLine
                .Split(separator)
                .Select(h => h.Trim().Trim('"').ToLowerInvariant())
                .ToList();

            if (langs.Count < 2)
                throw new PluginMisconfigurationException("File must contain at least two language codes in header.");

            var pivotLang = langs[0];
            var entriesList = new List<string[]>();
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var parts = line.Split(separator).Select(p => p.Trim().Trim('"')).ToArray();
                if (parts.Length == langs.Count)
                    entriesList.Add(parts);
            }

            foreach (var targetLang in langs.Skip(1))
            {
                var targetIndex = langs.IndexOf(targetLang);
                var tsv = string.Join("\n", entriesList.Select(e => $"{e[0]}\t{e[targetIndex]}"));
                var entriesText = ext == ".tsv" ? tsv : tsv.Replace("\t", ",");

                var body = new
                {
                    source_lang = pivotLang,
                    target_lang = targetLang,
                    entries = entriesText,
                    entries_format = ext.Trim('.')
                };
                var restReq = new RestRequest($"https://api.deepl.com/v3/glossaries/{request.GlossaryId}/dictionaries", Method.Put)
                    .AddJsonBody(body);
                var resp = await RestClient.ExecuteAsync<AddOrReplaceDictionaryResult>(restReq).ConfigureAwait(false);
                if (!resp.IsSuccessful)
                    throw new PluginApplicationException($"Multilingual dictionary error: {resp.StatusCode} – {resp.Content}");
                var dataRes = resp.Data!;
                lastSource = dataRes.SourceLang;
                lastTarget = dataRes.TargetLang;
                lastCount = dataRes.EntryCount;
            }
        }
        else
        {
            throw new PluginMisconfigurationException($"Unsupported format: {ext}");
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

    [Action("Export glossary (multilingual)", Description = "Export multilingual glossary")]
    public async Task<ExportGlossaryResponse> ExportGlossaryV3([ActionParameter] GlossaryRequest request)
    {
        var metaReq = new RestRequest($"https://api.deepl.com/v3/glossaries/{request.GlossaryId}", Method.Get);
        var metaResp = await RestClient.ExecuteAsync<GetGlossaryMetadataResult>(metaReq).ConfigureAwait(false);
        if (!metaResp.IsSuccessful)
            throw new PluginApplicationException($"Error: {metaResp.StatusCode} – {metaResp.Content}");
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
                throw new PluginApplicationException($"Multilingual entries error: {entriesResp.StatusCode} – {entriesResp.Content}");
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
        await using var tbxStream = blackbirdGlossary.ConvertToTbx();
        return new ExportGlossaryResponse
        {
            File = await fileManagementClient.UploadAsync(
                tbxStream,
                MediaTypeNames.Application.Xml,
                $"{metadata.Name}.tbx")
        };
    }

    [Action("Search glossaries", Description = "Search all glossaries")]
    public async Task<ListGlossariesResponse> ListGlossaries()
    {
        var glossary = await ErrorHandler.ExecuteWithErrorHandlingAsync(async () => await Client.ListGlossariesAsync());
        return new(glossary.Select(x => new GlossaryEntity(x)));
    }

    [Action("Get glossary details", Description = "Get details of a specific glossary")]
    public async Task<GlossaryEntity> GetGlossary([ActionParameter] GlossaryRequest input)
    {
        var glossary = await ErrorHandler.ExecuteWithErrorHandlingAsync(async () => await Client.GetGlossaryAsync(input.GlossaryId));
        return new(glossary);
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
        var blackbirdGlossary = await glossaryStream.ConvertFromTbx();
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

        if (glossaryValues.Count == 0)
            return (null, request.Name ?? blackbirdGlossary.Title!);

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

    private static string? NormalizeAndValidateLang(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;

        code = code.ToLowerInvariant().Trim();

        var exactMatch = LanguageConstants.TargetLanguages.Keys
            .FirstOrDefault(k => string.Equals(k, code, StringComparison.InvariantCultureIgnoreCase));

        if (exactMatch != null)
            return exactMatch.Split('-')[0].ToLowerInvariant();

        var parts = code.Split('-');
        var baseCode = parts[0];

        var isBaseSupported = LanguageConstants.TargetLanguages.Keys
            .Any(k => k.StartsWith(baseCode, StringComparison.InvariantCultureIgnoreCase));

        if (isBaseSupported)
            return baseCode;

        return null;
    }

    private async Task<(byte[] bytes, string extension)> DownloadGlossaryFile(FileReference file)
    {
        await using var stream = await fileManagementClient.DownloadAsync(file);
        await using var memStream = new MemoryStream();
        await stream.CopyToAsync(memStream).ConfigureAwait(false);

        var bytes = memStream.ToArray();
        var ext = Path.GetExtension(file.Name).ToLowerInvariant();

        return (bytes, ext);
    }

    private async Task<(string Name, string PivotLang, List<string> Langs)> ParseGlossaryMetadata(
        byte[] fileBytes, string ext, string? requestName, string fileName)
    {
        string glossaryName;
        string rawPivotLang;
        List<string> fileLangs;

        if (ext == ".tbx")
        {
            var xdoc = XDocument.Load(new MemoryStream(fileBytes));
            XNamespace ns = xdoc.Root.GetDefaultNamespace();

            glossaryName = string.IsNullOrWhiteSpace(requestName)
                ? xdoc.Descendants(ns + "title").FirstOrDefault()?.Value ?? Path.GetFileNameWithoutExtension(fileName)
                : requestName;

            fileLangs = xdoc.Descendants(ns + "langSec")
                .Attributes(XNamespace.Xml + "lang")
                .Select(a => a.Value)
                .Distinct()
                .ToList();

            rawPivotLang = xdoc.Root.Attribute(XNamespace.Xml + "lang")?.Value ?? fileLangs.First();
        }
        else if (ext == ".csv" || ext == ".tsv")
        {
            await using var cueStream = new MemoryStream(fileBytes);
            using var reader = new StreamReader(cueStream);
            var headerLine = await reader.ReadLineAsync();
            var delimiter = ext == ".csv" ? ',' : '\t';
            var header = headerLine?.Split(delimiter);

            if (header == null || header.Length < 2)
                throw new PluginMisconfigurationException("File must contain at least two language codes in header.");

            fileLangs = header.Select(l => l.Trim('"')).ToList();
            rawPivotLang = fileLangs[0];

            glossaryName = string.IsNullOrWhiteSpace(requestName) ? Path.GetFileNameWithoutExtension(fileName) : requestName;
        }
        else
            throw new PluginMisconfigurationException($"Unsupported format: {ext}");

        return (glossaryName, rawPivotLang, fileLangs);
    }

    private async Task<(List<object> Payload, List<string> Warnings)> BuildDictionariesPayload(
        byte[] fileBytes, string ext, string rawPivotLang, string validPivotLang,
        List<string> fileLangs, ImportMultilingualGlossaryRequest request)
    {
        var payloads = new List<object>();
        var warnings = new List<string>();

        foreach (var rawTargetLang in fileLangs.Where(l => !string.Equals(l, rawPivotLang, StringComparison.OrdinalIgnoreCase)))
        {
            var validTargetLang = NormalizeAndValidateLang(rawTargetLang);

            if (validTargetLang == null)
            {
                warnings.Add($"Language '{rawTargetLang}' is not supported by DeepL Glossaries and was ignored.");
                continue;
            }

            if (!string.Equals(rawTargetLang, validTargetLang, StringComparison.OrdinalIgnoreCase))
                warnings.Add($"Language '{rawTargetLang}' was normalized to '{validTargetLang}'.");

            string? tsvContent = await ExtractPairContent(fileBytes, ext, rawPivotLang, rawTargetLang, request);

            if (!string.IsNullOrWhiteSpace(tsvContent))
            {
                payloads.Add(new
                {
                    source_lang = validPivotLang,
                    target_lang = validTargetLang,
                    entries = tsvContent,
                    entries_format = "tsv"
                });
            }
            else
            {
                warnings.Add(
                    $"Language '{rawTargetLang}' had no valid matching entries with source '{rawPivotLang}' and was skipped."
                );
            }
        }

        if (payloads.Count == 0)
            throw new PluginMisconfigurationException($"No valid glossary dictionaries could be created. Warnings: {string.Join("; ", warnings)}");

        return (payloads, warnings);
    }

    private async Task<string?> ExtractPairContent(
        byte[] fileBytes, string ext, string rawPivot, string rawTarget,
        ImportMultilingualGlossaryRequest request)
    {
        if (ext == ".tbx")
        {
            var reqPair = new ImportGlossaryRequest
            {
                File = request.File,
                SourceLanguageCode = rawPivot,
                TargetLanguageCode = rawTarget,
                Name = request.Name
            };

            await using var pairStream = new MemoryStream(fileBytes);
            var (entriesPair, _) = await GetEntriesFromTbx(reqPair, pairStream);
            return entriesPair?.ToTsv();
        }
        else
        {
            await using var readStream = new MemoryStream(fileBytes);
            return await ExtractCsvTsvPair(readStream, ext, rawPivot, rawTarget);
        }
    }

    private async Task<CreateGlossaryV3Result> CreateGlossary(
        string glossaryName, List<object> dictionariesPayload, List<string> warnings)
    {
        var body = new
        {
            name = glossaryName,
            glossaryRequestParameters = new { },
            dictionaries = dictionariesPayload.ToArray()
        };

        var jsonBody = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        var restReq = new RestRequest("https://api.deepl.com/v3/glossaries", Method.Post)
            .AddParameter("application/json", jsonBody, ParameterType.RequestBody);

        var resp = await RestClient.ExecuteAsync<CreateGlossaryV3Result>(restReq).ConfigureAwait(false);

        if (!resp.IsSuccessful)
        {
            throw new PluginApplicationException(
                $"DeepL API Error: {resp.StatusCode} – {resp.Content}. (Internal Warnings: {string.Join("; ", warnings)})"
            );
        }

        return resp.Data!;
    }

    private async static Task<string?> ExtractCsvTsvPair(Stream fileStream, string ext, string pivotLang, string targetLang)
    {
        fileStream.Position = 0;
        using var reader = new StreamReader(fileStream, leaveOpen: true);

        var delimiter = ext == ".csv" ? ',' : '\t';
        var headerLine = await reader.ReadLineAsync();

        if (string.IsNullOrWhiteSpace(headerLine)) return null;

        var header = headerLine.Split(delimiter)
                               .Select(x => x.Trim().Trim('"').ToLowerInvariant())
                               .ToList();

        int pivotIndex = header.IndexOf(pivotLang.ToLowerInvariant());
        int targetIndex = header.IndexOf(targetLang.ToLowerInvariant());

        if (pivotIndex == -1)
            pivotIndex = header.FindIndex(h => h.StartsWith(pivotLang.Split('-')[0], StringComparison.InvariantCultureIgnoreCase));

        if (targetIndex == -1)
            targetIndex = header.FindIndex(h => h.StartsWith(targetLang.Split('-')[0], StringComparison.InvariantCultureIgnoreCase));

        if (pivotIndex == -1 || targetIndex == -1) return null;

        var entries = new List<string>();
        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            var parts = line.Split(delimiter).Select(p => p.Trim().Trim('"')).ToArray();

            if (parts.Length > Math.Max(pivotIndex, targetIndex))
            {
                var source = parts[pivotIndex];
                var target = parts[targetIndex];

                if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(target))
                    entries.Add($"{source}\t{target}");
            }
        }

        return entries.Count != 0 ? string.Join("\n", entries) : null;
    }
}