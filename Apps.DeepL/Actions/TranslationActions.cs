using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Apps.DeepL.Requests;
using Apps.DeepL.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Xliff.Utils;
using Blackbird.Xliff.Utils.Converters;
using Blackbird.Xliff.Utils.Extensions;
using Blackbird.Xliff.Utils.Models;
using DeepL;
using DocumentFormat.OpenXml.Office2010.Word;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Primitives;

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
        var tuple = await GetFileAndXliffDocumentAsync(request);
        var file = tuple.Item1;
        var xliffDocument = tuple.Item2;
    
        var outputStream = new MemoryStream();

        
        await Client.TranslateDocumentAsync(file, request.File.Name, outputStream, request.SourceLanguage,
            request.TargetLanguage, CreateDocumentTranslateOptions(request));

        var newFileName = request.File.Name;

        if (request.TranslateFileName.HasValue && request.TranslateFileName.Value)
        {
            var translateResponse = await Translate(new TextTranslationRequest
            {
                Formal = request.Formal,
                GlossaryId = request.GlossaryId,
                SourceLanguage = request.SourceLanguage,
                TargetLanguage = request.TargetLanguage,
                Text = request.File.Name,
            });
            newFileName = translateResponse.TranslatedText;
        }
        
        var memoryStream = new MemoryStream(outputStream.GetBuffer());
        memoryStream.Position = 0;

        XDocument? result = null;
        if (xliffDocument != null)
        {
            if (tuple.Item2?.Version == "1.2")
            {
                var finalFile = ConvertBack(memoryStream);
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(finalFile));
                var uploadedFinalFile = await _fileManagementClient.UploadAsync(stream, request.File.ContentType, newFileName);
                return new FileResponse { File = uploadedFinalFile };
            }
            var xliffDocument21 = memoryStream.ToXliffDocument(new Blackbird.Xliff.Utils.Models.XliffConfig
            {
                CopyAttributes = true,
                IncludeInlineTags = true,
                RemoveWhitespaces = true,
                DontChangeTags = true
            });
            result = xliffDocument?.UpdateTranslationUnits(xliffDocument21.TranslationUnits);
        }

        var outputFileStream = result?.ToStream(ignoreAllFormatting: true) ?? memoryStream;
        var uploadedFile = await _fileManagementClient.UploadAsync(outputFileStream, request.File.ContentType, newFileName);
        return new FileResponse { File = uploadedFile };
    }

    private string ConvertBack(MemoryStream memoryStream)
    {
        var parsed = new XliffDocument();
        var xliff = XDocument.Load(memoryStream);
        XNamespace ns = "urn:oasis:names:tc:xliff:document:2.0";
        parsed.SourceLanguage = xliff.Root.Attribute("srcLang")?.Value;
        parsed.TargetLanguage = xliff.Root.Attribute("trgLang")?.Value;
        

        foreach (var unit in xliff.Descendants(ns + "unit"))
        {
            var id = unit.Attribute("id")?.Value;
            var sourceElement = unit.Descendants(ns + "source").FirstOrDefault();
            var targetElement = unit.Descendants(ns + "target").FirstOrDefault();

            string source = "";
            string target = "";

            var originalDataElement = unit.Descendants(ns + "originalData")?.FirstOrDefault();
            if (originalDataElement != null) 
            {
                var originalDataList = originalDataElement.Elements().ToList();

                source = RemoveExtraNewLines(Regex.Replace(sourceElement.ToString(), @"</?source(.*?)>", @""));
                target = RemoveExtraNewLines(Regex.Replace(targetElement.ToString(), @"</?target(.*?)>", @""));

                foreach (var tag in Regex.Matches(source, "<ph id=\"(.*?)\" dataRef=\"(.*?)\" />").Select(x => x.Value)) 
                {
                    var dataRef = Regex.Match(tag, "dataRef=\"(.*?)\"").Groups[1].Value;
                    string newValue = RemoveExtraNewLines(Regex.Replace(originalDataList.First(x => x.Attribute("id")?.Value == dataRef).ToString(), @"</?data(.*?)>", ""));
                    source = Regex.Replace(source, tag, newValue);
                    target = Regex.Replace(target, tag, newValue);
                }

                while (Regex.IsMatch(source, "<pc id=\"(.*?)\" dataRefStart=\"(.*?)\" dataRefEnd=\"(.*?)\">(.*?)</pc>")) 
                {
                   var match = Regex.Match(source, "<pc id=\"(.*?)\" dataRefStart=\"(.*?)\" dataRefEnd=\"(.*?)\">(.*?)</pc>");
                   var dataRef = match.Groups[2].Value;
                   var endDataRef = match.Groups[3].Value;
                   var bpt = RemoveExtraNewLines(Regex.Replace(originalDataList.First(x => x.Attribute("id")?.Value == dataRef).ToString(), @"</?data(.*?)>", ""));
                   var ept = RemoveExtraNewLines(Regex.Replace(originalDataList.First(x => x.Attribute("id")?.Value == endDataRef).ToString(), @"</?data(.*?)>", ""));
                   source = Regex.Replace(source,Regex.Escape(match.Value), bpt + match.Groups[4].Value + ept);
                }

                while (Regex.IsMatch(target, "<pc id=\"(.*?)\" dataRefStart=\"(.*?)\" dataRefEnd=\"(.*?)\">(.*?)</pc>"))
                {
                    var match = Regex.Match(target, "<pc id=\"(.*?)\" dataRefStart=\"(.*?)\" dataRefEnd=\"(.*?)\">(.*?)</pc>");
                    var dataRef = match.Groups[2].Value;
                    var endDataRef = match.Groups[3].Value;
                    var bpt = RemoveExtraNewLines(Regex.Replace(originalDataList.First(x => x.Attribute("id")?.Value == dataRef).ToString(), @"</?data(.*?)>", ""));
                    var ept = RemoveExtraNewLines(Regex.Replace(originalDataList.First(x => x.Attribute("id")?.Value == endDataRef).ToString(), @"</?data(.*?)>", ""));
                    target = Regex.Replace(target, Regex.Escape(match.Value), bpt + match.Groups[4].Value + ept);
                }

            }
            else
            {
                source = sourceElement.Value;
                target = targetElement.Value;
            }


            parsed.TranslationUnits.Add( new TranslationUnit 
            {
                Id = id,
                Source = source,
                Target = target
                
            });
        }

        return CreateDoc(parsed);
       
    }

    private string CreateDoc(XliffDocument parsed)
    {
        XNamespace ns = "urn:oasis:names:tc:xliff:document:1.2";
        var newXliff = new XDocument(
            new XElement(ns + "xliff", new XAttribute("version", "1.2"),
                new XElement(ns + "file",
                    new XAttribute("original","unknown"),
                    new XAttribute("source-language", parsed.SourceLanguage),
                    new XAttribute("target-language", parsed.TargetLanguage),
                    new XAttribute("datatype","plaintext"),
                    new XElement(ns + "header"),
                    new XElement(ns + "body")
                )
            )
        );
        string content = CreateBody(parsed.TranslationUnits, newXliff);
        return content;
    }

    private string CreateBody(List<TranslationUnit> translationUnits, XDocument newXliff)
    {
        string fileContent;
        Encoding encoding;
        var xliffStream = new MemoryStream();
        newXliff.Save(xliffStream);

        xliffStream.Position = 0;
        using (StreamReader inFileStream = new StreamReader(xliffStream))
        {
            encoding = inFileStream.CurrentEncoding;
            fileContent = inFileStream.ReadToEnd();
        }
        var body = new StringBuilder();
        foreach (var tu in translationUnits)
        {
            body.Append($"  <trans-unit id=\"" + tu.Id + "\">" + Environment.NewLine + "\t\t<source xml:space=\"preserve\">" + tu.Source + "</source>" + Environment.NewLine + "\t\t<target>" + tu.Target + "</target>" + Environment.NewLine + "	</trans-unit>" + Environment.NewLine);
        }
        fileContent = fileContent.Replace("<body />", "<body>" + Environment.NewLine + body.ToString() + Environment.NewLine + "</body>");
        return fileContent;
    }

    private static string RemoveExtraNewLines(string originalString)
    {
        if (!string.IsNullOrWhiteSpace(originalString))
        {
            var to_modify = originalString;
            to_modify = Regex.Replace(to_modify, @"\r?\n(\s+)?", "", RegexOptions.Multiline);
            return to_modify;
        }
        else
        {
            return string.Empty;
        }
    }
    private TextTranslateOptions CreateTextTranslateOptions(TextTranslationRequest request)
    {
        var options = new TextTranslateOptions
        {
            PreserveFormatting = request.PreserveFormatting == null || (bool)request.PreserveFormatting,
            Formality = request.Formal == null
                ? Formality.Default
                : (bool)request.Formal
                    ? Formality.PreferMore
                    : Formality.PreferLess,
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
                : (bool)request.Formal
                    ? Formality.PreferMore
                    : Formality.PreferLess,
            GlossaryId = request.GlossaryId,
        };
    }

    private async Task<(Stream, XliffDocument?)> GetFileAndXliffDocumentAsync(DocumentTranslationRequest request)
    {
        var fileStream = await _fileManagementClient.DownloadAsync(request.File);

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
                XliffDocument xliffDocument = memoryStream.ToXliffDocument(new Blackbird.Xliff.Utils.Models.XliffConfig
                {
                    CopyAttributes = true,
                    IncludeInlineTags = true,
                    RemoveWhitespaces = true,
                    DontChangeTags = true
                });

                //  var converted = Xliff12To21Converter.Convert(xliffDocument);
                // return (converted.ToStream(ignoreAllFormatting: true), xliffDocument);

                var converted = Convert12To21(xliffDocument);
                var bytes = Encoding.Default.GetBytes(converted);
                var xliffStream = new MemoryStream(bytes);
                
                return (xliffStream, xliffDocument);
            }
        }

        return (memoryStream, null);
    }

    private string Convert12To21(XliffDocument document)
    {
        XNamespace xliff12Ns = "urn:oasis:names:tc:xliff:document:1.2";
        XNamespace xliff21Ns = "urn:oasis:names:tc:xliff:document:2.0";

        var sourceLang = document.SourceLanguage;
        var targetLang = document.TargetLanguage;

        var newRoot = new XElement(xliff21Ns + "xliff",
            new XAttribute("version", "2.1"),
            new XAttribute("srcLang", sourceLang),
            new XAttribute("trgLang", targetLang)
        );

        var fileElement = new XElement(xliff21Ns + "file");
        newRoot.Add(fileElement);
        var content = AddBody(newRoot.ToString(), document.TranslationUnits);
        return content;
    }

    private string AddBody(string file, List<TranslationUnit> tus)
    {
        var body = new StringBuilder();
        foreach (var tu in tus) 
        {
            if (tu.Tags.Count == 0)
            {
                body.Append($"    <unit id=\"{tu.Id}\">"+Environment.NewLine+ "      <segment>"+Environment.NewLine+$"        <source>{tu.Source}</source>"+Environment.NewLine+ "        <target />"+Environment.NewLine+ "      </segment>"+Environment.NewLine+ "    </unit>"+Environment.NewLine);
            }
            else 
            {
                string newSource = tu.Source;
                body.Append($"    <unit id=\"{tu.Id}\">" + Environment.NewLine);
                body.Append("   <originalData>" + Environment.NewLine);
                foreach (var tag in tu.Tags)
                {
                    body.Append($"   <data id=\"{tag.Type+tag.Id}\">{tag.Value}</data>" + Environment.NewLine);
                    if (tag.Type == "ph")
                    {
                        newSource = newSource.Replace(tag.Value, $"<ph id=\"{tag.Id}\" dataRef=\"{tag.Type+tag.Id}\" />");
                    }
                    if (tag.Type == "bpt")
                    {
                        string matchingEpt = "";
                        var EPTs = tu.Tags.Where(x => x.Type == "ept");
                        if (EPTs.Count() == 1) { matchingEpt = EPTs.First().Id; }
                        else 
                        {
                            if (EPTs.Any(x => x.Id == tag.Id)) { matchingEpt = EPTs.First(x => x.Id == tag.Id).Id; }
                            else 
                            {
                                var rid = Regex.Match(tag.Value, "rid=\"(.*?)\"").Value;
                                if (EPTs.Any(x => Regex.Match(x.Value, "rid=\"(.*?)\"").Value == rid))
                                { matchingEpt = EPTs.First(x => Regex.Match(x.Value, "rid=\"(.*?)\"").Value == rid).Id; }
                            }
                        }

                        newSource = newSource.Replace(tag.Value, $"<pc id=\"{tag.Id}\" dataRefStart=\"{tag.Type+tag.Id}\" dataRefEnd=\"{"ept"+matchingEpt}\">");

                    }
                    if (tag.Type == "ept")
                    {
                        newSource = newSource.Replace(tag.Value, "</pc>");
                    }
                }
                body.Append("   </originalData>" + Environment.NewLine);
                body.Append($"      <segment>" + Environment.NewLine + $"        <source>{newSource}</source>" + Environment.NewLine + "        <target />" + Environment.NewLine + "      </segment>" + Environment.NewLine + "    </unit>" + Environment.NewLine);


            }
        }
        var fileContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + file.Replace("<file />", "<file>" + Environment.NewLine + body.ToString() + Environment.NewLine + "</file>");
        return fileContent;
    }
}