using Apps.DeepL.Factories;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.DeepL.DataSourceHandlers
{
    public class GlossariesDataHandler : DeepLInvocable, IAsyncDataSourceHandler
    {
        public GlossariesDataHandler(InvocationContext invocationContext) : base(invocationContext)
        {
        }

        public async Task<Dictionary<string, string>> GetDataAsync(
            DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var glossaries = await Client.ListGlossariesAsync(cancellationToken);
            return glossaries.Where(x =>
                    context.SearchString == null ||
                    x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(x => x.GlossaryId, x => x.Name);
        }
    }
}
