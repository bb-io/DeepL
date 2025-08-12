using Apps.DeepL.Actions;
using Apps.DeepL.Requests.Xliff;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.DeepL.Base;

namespace Tests.DeepL;

[TestClass]
public class XliffTextActionsTests : TestBase
{
    [TestMethod]
    public async Task TranslateXliff_WithoutBatches_ReturnsTranslatedXliffFile()
    {
        // Arrange
        var actions = new XliffTextActions(InvocationContext, FileManager);
        var request = new XliffTranslationRequest
        {
            File = new FileReference
            {
                Name = "simple.xliff"
            },
            ModelType = "latency_optimized",
            TargetLanguage = "DE"
        };

        // Act
        var result = await actions.TranslateXliff(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.File);
        Assert.AreEqual("simple.xliff", result.File.Name);
    }

    
    [TestMethod]
    public async Task TranslateXliff_WithBatches_ReturnsTranslatedXliffFile()
    {
        // Arrange
        var actions = new XliffTextActions(InvocationContext, FileManager);
        var request = new XliffTranslationRequest
        {
            File = new FileReference
            {
                Name = "simple.xliff"
            },
            ModelType = "latency_optimized",
            UseBatches = true,
            TargetLanguage = "DE"
        };

        // Act
        var result = await actions.TranslateXliff(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.File);
        Assert.AreEqual("simple.xliff", result.File.Name);
    }

    [TestMethod]
    public async Task TranslateXliff_WithoutTargetLanguage_ShouldThrowMisconfigurationException()
    {
        // Arrange
        var actions = new XliffTextActions(InvocationContext, FileManager);
        var request = new XliffTranslationRequest
        {
            File = new FileReference
            {
                Name = "simple.xliff"
            },
            ModelType = "latency_optimized",
            TargetLanguage = string.Empty
        };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(async () => await actions.TranslateXliff(request));
    }
    
    [TestMethod]
    public async Task TranslateXliff_WithTxtFile_ShouldThrowMisconfigurationException()
    {
        // Arrange
        var actions = new XliffTextActions(InvocationContext, FileManager);
        var request = new XliffTranslationRequest
        {
            File = new FileReference
            {
                Name = "sample.txt"
            },
            ModelType = "latency_optimized",
            TargetLanguage = "DE"
        };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(async () => await actions.TranslateXliff(request));
    }
    
    [TestMethod]
    public async Task TranslateXliff_WithCsvFile_ShouldThrowMisconfigurationException()
    {
        // Arrange
        var actions = new XliffTextActions(InvocationContext, FileManager);
        var request = new XliffTranslationRequest
        {
            File = new FileReference
            {
                Name = "sample.csv"
            },
            ModelType = "latency_optimized",
            TargetLanguage = "DE"
        };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(async () => await actions.TranslateXliff(request));
    }

    [TestMethod]
    public async Task TranslateXliff_OnlyEmptyTargets_PreservesExistingTargets()
    {
        // Arrange
        var actions = new XliffTextActions(InvocationContext, FileManager);
        var request = new XliffTranslationRequest
        {
            File = new FileReference
            {
                Name = "simple_pretranslated.xliff"
            },
            ModelType = "latency_optimized",
            TargetLanguage = "DE",
            TranslateOnlyEmptyUnits = true,
        };

        // Act
        var result = await actions.TranslateXliff(request);

        // Assert
        Assert.AreEqual("simple_pretranslated.xliff", result.File.Name);
    }
}
