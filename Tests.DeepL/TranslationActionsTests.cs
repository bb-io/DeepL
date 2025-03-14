﻿using Apps.DeepL.Actions;
using Apps.DeepL.Requests;
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
        var result = await _actions.TranslateDocument(
            new DocumentTranslationRequest 
            { 
                File = new Blackbird.Applications.Sdk.Common.Files.FileReference 
                { 
                    Name = "testEmpty.txt" 
                },
                TargetLanguage = "es"
            });

        Assert.IsNotNull(result);
    }
}
