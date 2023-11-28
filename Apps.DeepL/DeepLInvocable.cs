using Apps.DeepL.Factories;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using DeepL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.DeepL
{
    public class DeepLInvocable : BaseInvocable
    {
        protected Translator Client { get; }
        public DeepLInvocable(InvocationContext invocationContext) : base(invocationContext) 
        {
            Client = TranslatorFactory.GetTranslator(invocationContext.AuthenticationCredentialsProviders);
        }
    }
}
