using Apps.DeepL.Actions;
using Apps.DeepL.Models;
using Apps.DeepL.Requests;
using Blackbird.Applications.Sdk.Common.Files;
using Newtonsoft.Json;
using Tests.DeepL.Base;

namespace Tests.DeepL;

[TestClass]
public class GlossaryActionsTests :TestBase
{
    [TestMethod]
    public async Task ImportGlossary_IsSuccess()
    {
        // Arrange
        var action = new GlossaryActions(InvocationContext, FileManager);
        var input = new ImportGlossaryRequest 
        { 
            File = new FileReference { Name = "samplev2.tbx" },
            SourceLanguageCode = "en",
            TargetLanguageCode = "fr"
        };

        // Act
        var result = await action.ImportGlossary(input);

        // Assert
        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ImportGlossaryV3_IsSuccess()
    {
        // Arrange
        var action = new GlossaryActions(InvocationContext, FileManager);
        var input = new ImportMultilingualGlossaryRequest
        {
            File = new FileReference { Name = "samplev3.tbx" },
        };

        // Act
        var result = await action.ImportGlossaryV3(input);

        // Assert
        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ExportGlossary_IssSuccess()
    {
        var action = new GlossaryActions(InvocationContext, FileManager);

        var result = await action.ExportGlossary(new GlossaryRequest { GlossaryId= "3730b484-8876-4236-bad5-05d426c85389",

        });

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ExportGlossaryNEW_IssSuccess()
    {
        var action = new GlossaryActions(InvocationContext, FileManager);

        var result = await action.ExportGlossaryUniversal(new ExportUniversalGlossaryRequest
        {
            GlossaryId = "af7a5c66-5264-4ca2-9408-52577b039b17",
             //Version= "v2"
        });

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ImportGlossaryNEW_IsSuccess()
    {
        // Arrange
        var action = new GlossaryActions(InvocationContext, FileManager);
        var input = new ImportUniversalGlossaryRequest
        {
            File = new FileReference { Name = "sample.csv" },
            SourceLanguageCode = "en",
            TargetLanguageCode = "es",
            Name = "Test Glossary from CSV 1401",
        };

        // Act
        var result = await action.ImportGlossaryUniversal(input);

        // Assert
        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task UpdatetGlossaryV3_IssSuccess()
    {
        var action = new GlossaryActions(InvocationContext, FileManager);

        var result = await action.UpdateDictionaryV3(new UpdateGlossaryRequest
        {
            GlossaryId = "51e21b1e-9d70-4b6b-9519-d1050a175c88",
            File = new FileReference { Name = "test.csv" }
        });

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ExportGlossaryv3_IssSuccess()
    {
        var action = new GlossaryActions(InvocationContext, FileManager);

        var result = await action.ExportGlossaryV3(new GlossaryRequest
        {
            GlossaryId = "2ae0905b-ed51-42f2-8abf-f0804b1b7780",

        });
        Assert.IsNotNull(result);
    }
}
