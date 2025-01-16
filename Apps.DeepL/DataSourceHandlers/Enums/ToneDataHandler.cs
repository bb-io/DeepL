using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.DeepL.DataSourceHandlers.Enums;
public class ToneDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return new List<DataSourceItem>()
        {
            new DataSourceItem("default", "Default"),
            new DataSourceItem("prefer_enthusiastic", "Enthusiastic"),
            new DataSourceItem("prefer_friendly", "Friendly"),
            new DataSourceItem("prefer_confident", "Confident"),
            new DataSourceItem("prefer_diplomatic", "Diplomatic"),
        };
    }
}

