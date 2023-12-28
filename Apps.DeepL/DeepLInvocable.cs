using Apps.DeepL.Factories;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using DeepL;

namespace Apps.DeepL;

public class DeepLInvocable : BaseInvocable
{
    protected Translator Client { get; }
    public DeepLInvocable(InvocationContext invocationContext) : base(invocationContext) 
    {
        Client = TranslatorFactory.GetTranslator(invocationContext.AuthenticationCredentialsProviders);
    }
}