using Apps.DeepL.Actions;
using Apps.DeepL.Requests;
using Apps.DeepL.Requests.Content;
using Blackbird.Applications.Sdk.Common.Files;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.DeepL.Base;

namespace Tests.DeepL;

[TestClass]
public class ContentTests : TestBase
{
    [TestMethod]
    public async Task Translate_Contentful_html()
    {
        var actions = new ContentActions(InvocationContext, FileManager);
        var file = new FileReference { Name = "contentful.html" };
        var result = await actions.TranslateContent(new ContentTranslationRequest() { File = file, SourceLanguage = "en", TargetLanguage = "nl" });

        Assert.AreEqual(result.File.Name, "contentful.html.xliff");
    }
}
