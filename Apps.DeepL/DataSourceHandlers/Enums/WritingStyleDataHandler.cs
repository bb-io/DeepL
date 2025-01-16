using Apps.DeepL.Constants;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.DeepL.DataSourceHandlers.Enums;
public class WritingStyleDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return new List<DataSourceItem>()
        {
            new DataSourceItem("default", "Default"),
            new DataSourceItem("prefer_simple", "Simple"),
            new DataSourceItem("prefer_business", "Business"),
            new DataSourceItem("prefer_academic", "Academic"),
            new DataSourceItem("prefer_casual", "Casual"),
        };
    }
}
