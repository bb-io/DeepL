using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Sdk.DataSourceHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.DeepL.DataSourceHandlers
{
    public class GlossaryLanguageDataHandler : EnumDataHandler
    {
        protected override Dictionary<string, string> EnumValues => new()
        {
            {"de", "German"},
            {"en", "English"},
            {"es", "Spanish"},
            {"fr", "French"},
            {"it", "Italian"},
            {"ja", "Japanese"},
            {"nl", "Dutch"},
            {"pl", "Polish"},
            {"pt", "Portuguese"},
            {"ru", "Russian"},
            {"zh", "Chinese"},
        };
    }
}
