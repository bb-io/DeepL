using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.DeepL.DataSourceHandlers;

public class GlossariesDataHandler(InvocationContext invocationContext)
    : DeepLInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var glossaries = await Client.ListGlossariesAsync(cancellationToken);
        return glossaries.Where(x =>
                context.SearchString == null ||
                x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(x => new DataSourceItem(x.GlossaryId, x.Name));
    }
}