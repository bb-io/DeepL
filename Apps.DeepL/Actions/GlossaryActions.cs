using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.DeepL.Actions;

[ActionList]
public class GlossaryActions : DeepLInvocable
{
    public GlossaryActions(InvocationContext invocationContext) : base(invocationContext) { }

    //[Action("Create glossary", Description = "Create a new glossary")]
    //public async Task<NewGlossaryResponse> CreateGlossary([ActionParameter] CreateGlossaryRequest request)
    //{
    //    if (request.File.ContentType != "text/csv")
    //        throw new Exception("The file type should be text/csv");

    //    var dict = new Dictionary<string, string> {  };
    //    var result = await Client.CreateGlossaryAsync(request.Name, request.SourceLanguage, request.TargetLanguage, new GlossaryEntries(dict));
    //    await Client.WaitUntilGlossaryReadyAsync(result.GlossaryId);
    //    return new NewGlossaryResponse
    //    {
    //        GossaryId = result.GlossaryId,
    //        Name = result.Name,
    //        SourceLanguageCode = result.SourceLanguageCode,
    //        TargetLanguageCode = result.TargetLanguageCode,
    //        EntryCount = result.EntryCount,
    //    };
    //}

    //[Action("Download glossary", Description = "Download a specified glossary for use in other apps")]
    //public async Task<DownloadGlossaryResponse> DownloadGlossary([ActionParameter] GlossaryRequest request)
    //{
    //    var result = await Client.GetGlossaryEntriesAsync(request.GlossaryId);
    //    return new DownloadGlossaryResponse
    //    {
    //    };
    //}

    //[Action("Delete glossary", Description = "Delete a glossary")]
    //public async Task DeleteGlossary([ActionParameter] GlossaryRequest request)
    //{
    //    await Client.DeleteGlossaryAsync(request.GlossaryId);
    //}
}