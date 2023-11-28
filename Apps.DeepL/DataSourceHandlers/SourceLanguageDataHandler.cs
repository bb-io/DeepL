using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.DeepL.DataSourceHandlers;

public class SourceLanguageDataHandler : DeepLInvocable, IAsyncDataSourceHandler
{
    public SourceLanguageDataHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(
        DataSourceContext context,
        CancellationToken cancellationToken)
    {

        var languages = await Client.GetSourceLanguagesAsync(cancellationToken);
        return languages.Where(x =>
                context.SearchString == null ||
                x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.Code, x => x.Name);
    }
}