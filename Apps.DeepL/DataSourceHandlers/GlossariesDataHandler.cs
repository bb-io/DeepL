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
        var glossariesv3Request = new RestRequest("https://api.deepl.com/v3/glossaries", Method.Get);
        var glossariesv3 = await RestClient.ExecuteAsync<ListGlossariesV3Response>(glossariesv3Request).ConfigureAwait(false);
        if (!glossariesv3.IsSuccessful)
            throw new PluginApplicationException($"{glossariesv3.StatusCode} – {glossariesv3.Content}");

        var glossariesv2 = await Client.ListGlossariesAsync(ct);

        var v2Items = glossariesv2.Select(x => new DataSourceItem(x.GlossaryId, x.Name));
        var v3Items = glossariesv3.Data?.Glossaries.Select(x => new DataSourceItem(x.GlossaryId, x.Name)) ?? [];

        var items = new List<DataSourceItem>();
        items.AddRange(v2Items);
        items.AddRange(v3Items);

        if (!string.IsNullOrWhiteSpace(context.SearchString))
            return items.Where(x => x.DisplayName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));

        return items;
    }
}