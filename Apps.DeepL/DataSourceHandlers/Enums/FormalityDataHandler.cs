using Blackbird.Applications.Sdk.Common.Dictionaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.DeepL.DataSourceHandlers.Enums
{
    public class FormalityDataHandler : IStaticDataSourceHandler
    {
        public Dictionary<string, string> GetData() => new()
        {
            { "default", "Default" },
            { "more", "More" },
            { "less", "Less" },
            { "prefer_more", "Prefer more" },
            { "prefer_less", "Prefer less" },
        };
    }
}
