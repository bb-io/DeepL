using Apps.DeepL.Constants;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.DeepL.DataSourceHandlers.Enums;
public class ImproveLanguageDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return LanguageConstants.TargetLanguages.Select(x => new DataSourceItem(x.Key, x.Value));
    }
}
