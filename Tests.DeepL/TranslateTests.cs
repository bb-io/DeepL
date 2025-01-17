using Apps.DeepL.Actions;
using Apps.DeepL.Requests;
using Blackbird.Applications.Sdk.Common.Invocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.DeepL.Base;

namespace Tests.DeepL;

[TestClass]
public class TranslateTests : TestBase
{
    private const string ExampleText = "Hello world!";

    [TestMethod]
    public async Task Base_translate_works()
    {
        var actions = new TranslationActions(InvocationContext, FileManager);

        var result = await actions.Translate(new TextTranslationRequest { Text = ExampleText, TargetLanguage = "nl"});
        Console.WriteLine(result.TranslatedText);
        Assert.IsNotNull(result.TranslatedText);
    }

    [TestMethod]
    public async Task Empty_text_throws_misconfiguration()
    {
        var actions = new TranslationActions(InvocationContext, FileManager);
        await Throws.MisconfigurationException(() => actions.Translate(new TextTranslationRequest { Text = string.Empty, TargetLanguage = "nl" }));
    }
}
