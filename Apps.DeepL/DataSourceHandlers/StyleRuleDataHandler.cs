using Apps.DeepL.Entities;
using Apps.DeepL.Responses.StyleRules;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.DeepL.DataSourceHandlers;

public class StyleRuleDataHandler(InvocationContext invocationContext)
    : DeepLInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        var styleRules = new List<StyleRuleEntity>();
        var page = 0;
        const int pageSize = 25;

        while(true)
        {
            var request = new RestRequest("https://api.deepl.com/v3/style_rules", Method.Get);
            request.AddQueryParameter("page", page);
            request.AddQueryParameter("page_size", pageSize);
            request.AddQueryParameter("detailed", "false");
            var response = await RestClient.ExecuteAsync<ListStyleRulesResponse>(request).ConfigureAwait(false);
            if (!response.IsSuccessful)
                throw new PluginApplicationException($"{response.StatusCode} – {response.Content}");

            var latestResult = response.Data?.StyleRules ?? [];
            if (latestResult.Count() == 0)
                break;

            styleRules.AddRange(latestResult);

            if (latestResult.Count() < pageSize)
                break;

            page++;
        }
        

        var items = styleRules.Select(x => new DataSourceItem(x.StyleId, $"{x.Name} ({x.Language})")) ?? [];
        if (!string.IsNullOrWhiteSpace(context.SearchString))
            return items.Where(x => x.DisplayName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));

        return items;
    }
}
