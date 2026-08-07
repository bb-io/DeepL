using Apps.DeepL.Requests;
using Apps.DeepL.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using DeepL;

namespace Apps.DeepL.Actions;

[ActionList]
public class GetUsageActionActions : DeepLInvocable
{

    public GetUsageActionActions(InvocationContext invocationContext) : base( invocationContext )
    {

    }

    [Action("GetUsage", Description = "Get usage information")]
    public async Task<GetUsageResponse> GetUsage()
    {
        var result = await Client.GetUsageAsync();
        var response = new GetUsageResponse();
        if (result.Character != null)
        {
            response.CharacterCount = result.Character.Count;
            response.CharacterLimit = result.Character.Limit;
            response.CharacterLimitReached = result.Character.LimitReached;
        }
        if (result.Document != null)
        {
            response.DocumentCount = result.Document.Count;
            response.DocumentLimit = result.Document.Limit;
            response.DocumentLimitReached = result.Document.LimitReached;
        }
        if (result.TeamDocument != null)
        {
            response.TeamDocumentCount = result.TeamDocument.Count;
            response.TeamDocumentLimit = result.TeamDocument.Limit;
            response.TeamDocumentLimitReached = result.TeamDocument.LimitReached;
        }
        return response;
    }
}