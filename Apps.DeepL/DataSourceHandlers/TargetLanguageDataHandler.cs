using Apps.DeepL.Factories;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.DeepL.DataSourceHandlers;

public class TargetLanguageDataHandler : DeepLInvocable, IAsyncDataSourceHandler
{
    public TargetLanguageDataHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(
        DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var languages = await Client.GetTargetLanguagesAsync(cancellationToken);
        return languages.Where(x =>
                context.SearchString == null ||
                x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.Code, x => x.Name);
    }
}