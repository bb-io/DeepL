using RestSharp;
using Apps.DeepL.Responses.Glossaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.DeepL.DataSourceHandlers;

public class GlossariesDataHandler(InvocationContext invocationContext)
    : DeepLInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        var request = new RestRequest("https://api.deepl.com/v3/glossaries", Method.Get);
        var response = await RestClient.ExecuteAsync<ListGlossariesV3Response>(request).ConfigureAwait(false);
        if (!response.IsSuccessful)
            throw new PluginApplicationException($"{response.StatusCode} – {response.Content}");

        var items = response.Data?.Glossaries.Select(x => new DataSourceItem(x.GlossaryId, x.Name)) ?? [];
        if (!string.IsNullOrWhiteSpace(context.SearchString))
            return items.Where(x => x.DisplayName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));

        return items;
    }
}