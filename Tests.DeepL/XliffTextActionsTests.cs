using Apps.DeepL.Actions;
using Apps.DeepL.Requests.Xliff;
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
            ModelType = "latency_optimized"
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
            UseBatches = true
        };

        // Act
        var result = await actions.TranslateXliff(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.File);
        Assert.AreEqual("simple.xliff", result.File.Name);
    }
}
