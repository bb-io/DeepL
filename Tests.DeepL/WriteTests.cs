using Apps.DeepL.Actions;
using Apps.DeepL.Connections;
using Apps.DeepL.Requests;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.DeepL.Base;

namespace Tests.DeepL;

[TestClass]
public class WriteTests : TestBase
{
    private const string ExampleText = "What is good code? Within the field of software development there is probably no other topic that has been as furiously overdebated. Many have understood already that good code is about being easily readable and comprehensible; that good code is performant and consistent, that it is reusable and maintainable. I’m sure you have heard these words many times before. Most articles and talks on this topic will provide numerous heuristics of good code. Comments - not too few, not too many; indentation - use it at all times but don’t go too deep; naming - be explicit but don’t give your variables lengthy names! And for Linus’ sake separate your concerns! I think that for all but the people who are just emerging themselves in the world of software development these ideas are widely known and accepted. Yet in practice we can have endless discussions about them. I sense that this is because the sheer plurality of heuristics we ascribe to ‘good code’ inhibits us from consistently applying all of them. Every developer weighs these heuristics differently and will thus experience the ‘goodness’ of every piece of code differently based on their own opinions.";
    
    [TestMethod]
    public async Task Base_write_works()
    {
        var actions = new WriteActions(InvocationContext);

        var result = await actions.Improve(new ImproveRequest { Text = ExampleText });
        Console.WriteLine(result.Text);
        Assert.IsNotNull(result.Text);
    }

    [TestMethod]
    public async Task Empty_text_throws_misconfiguration()
    {
        var actions = new WriteActions(InvocationContext);
        await Throws.MisconfigurationException(() => actions.Improve(new ImproveRequest { Text = string.Empty }));
    }

    [TestMethod]
    public async Task Base_write_with_invalid_language_throws()
    {
        var actions = new WriteActions(InvocationContext);

        await Throws.MisconfigurationException(
            () => actions.Improve(new ImproveRequest { Text = ExampleText, TargetLanguage = "nl" })
        );
    }

    [TestMethod]
    public async Task Base_write_with_with_prefer_writing_style()
    {
        var actions = new WriteActions(InvocationContext);
        var result = await actions.Improve(new ImproveRequest { Text = ExampleText, WritingStyle = "prefer_business", TargetLanguage = "es" });
        Console.WriteLine(result.Text);
        Assert.IsNotNull(result.Text);
    }

    [TestMethod]
    public async Task Base_write_with_with_writing_style_and_tone_throws()
    {
        var actions = new WriteActions(InvocationContext);

        await Throws.MisconfigurationException(
            () => actions.Improve(new ImproveRequest { Text = ExampleText, WritingStyle = "business", Tone = "friendly" })
        );

    }
}
