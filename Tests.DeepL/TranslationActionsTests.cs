using Apps.DeepL.Actions;
using Apps.DeepL.Requests;
using Apps.DeepL.Requests.Content;
using Tests.DeepL.Base;

namespace Tests.DeepL;

[TestClass]
public class TranslationActionsTests : TestBase
{
    private const string ExampleText = "Hello world!";
    private TranslationActions _actions;

    [TestInitialize]
    public void Setup()
    {
        _actions = new TranslationActions(InvocationContext, FileManager);
    }

    [TestMethod]
    public async Task Translate_WithBasicInput_ReturnsTranslatedText()
    {
        var result = await _actions.Translate(new TextTranslationRequest 
        { 
            Text = ExampleText, 
            TargetLanguage = "nl"
        });
        
        Assert.IsNotNull(result.TranslatedText);
        Console.WriteLine(result.TranslatedText);
        Console.WriteLine(result.BilledCharacters);
        Assert.IsTrue(result.BilledCharacters > 0);
    }

    [TestMethod]
    public async Task Translate_Vietnamese()
    {
        var result = await _actions.Translate(new TextTranslationRequest
        {
            Text = ExampleText,
            TargetLanguage = "VI"
        });

        Assert.IsNotNull(result.TranslatedText);
        Console.WriteLine(result.TranslatedText);
        Console.WriteLine(result.BilledCharacters);
        Assert.IsTrue(result.BilledCharacters > 0);
    }

    [TestMethod]
    public async Task Translate_WithLatencyOptimizedModel_ReturnsTranslatedText()
    {
        var result = await _actions.Translate(new TextTranslationRequest 
        { 
            Text = ExampleText, 
            TargetLanguage = "nl", 
            ModelType = "latency_optimized"
        });
        
        Assert.IsNotNull(result.TranslatedText);
        Console.WriteLine(result.BilledCharacters);
        Assert.IsTrue(result.BilledCharacters > 0);
    }

    [TestMethod]
    public async Task Translate_WithQualityOptimizedModel_ReturnsTranslatedText()
    {
        var result = await _actions.Translate(new TextTranslationRequest 
        { 
            Text = ExampleText, 
            TargetLanguage = "FR", 
            ModelType = "quality_optimized"
        });
        
        Assert.IsNotNull(result.TranslatedText);
        Console.WriteLine(result.BilledCharacters);
        Assert.IsTrue(result.BilledCharacters > 0);
    }

    [TestMethod]
    public async Task Translate_WithEmptyText_ThrowsMisconfigurationException()
    {
        await Throws.MisconfigurationException(() => 
            _actions.Translate(new TextTranslationRequest 
            { 
                Text = string.Empty, 
                TargetLanguage = "nl" 
            }));
    }

    [TestMethod]
    public async Task TranslateDocument_WithValidFile_ReturnsTranslatedDocument()
    {
        var result = await _actions.TranslateContent(
            new ContentTranslationRequest
            { 
                File = new Blackbird.Applications.Sdk.Common.Files.FileReference 
                { 
                    Name = "contentful.html" 
                },
                TargetLanguage = "es"
            });

        Assert.IsNotNull(result);
        Console.WriteLine(result.BilledCharacters);
        Assert.IsTrue(result.BilledCharacters > 0);
    }

    [TestMethod]
    public async Task TranslateDocument_WithValidXliffFile_ReturnsTranslatedDocument()
    {
        var result = await _actions.TranslateContent(
            new ContentTranslationRequest
            { 
                File = new Blackbird.Applications.Sdk.Common.Files.FileReference 
                { 
                    Name = "contentful.html.xliff" 
                },
                TargetLanguage = "DE"
            });

        Assert.IsNotNull(result);
        Console.WriteLine(result.BilledCharacters);
        Assert.IsTrue(result.BilledCharacters > 0);
    }

    [TestMethod]
    public async Task TranslateDocument_WithPreleveragedFile_ReturnsSameDocument()
    {
        var result = await _actions.TranslateContent(
            new ContentTranslationRequest
            {
                File = new Blackbird.Applications.Sdk.Common.Files.FileReference
                {
                    Name = "preleveraged.xlf"
                },
                TargetLanguage = "nl"
            });

        Assert.IsNotNull(result);
        Console.WriteLine(result.BilledCharacters);
        Assert.IsTrue(result.BilledCharacters == 0);
    }

    [TestMethod]
    public async Task Translate_Contentful_return_xliff()
    {
        var result = await _actions.TranslateContent(
            new ContentTranslationRequest
            {
                File = new Blackbird.Applications.Sdk.Common.Files.FileReference
                {
                    Name = "The Loire Valley_en-US.html",
                },
                TargetLanguage = "nl",
            });

        Assert.IsNotNull(result);
        Console.WriteLine(result.BilledCharacters);
        Assert.IsTrue(result.BilledCharacters > 0);
    }

    [TestMethod]
    public async Task Translate_Zendesk_return_xliff()
    {
        var result = await _actions.TranslateContent(
            new ContentTranslationRequest
            {
                File = new Blackbird.Applications.Sdk.Common.Files.FileReference
                {
                    Name = "Multilingual AI Roundtable 2025 in Malmö!.html",
                },
                TargetLanguage = "nl",
            });

        Assert.IsNotNull(result);
        Console.WriteLine(result.BilledCharacters);
        Assert.IsTrue(result.BilledCharacters > 0);
    }
}
