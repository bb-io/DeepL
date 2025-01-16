using Apps.DeepL.Constants;
using Apps.DeepL.Factories;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using DeepL;
using DocumentFormat.OpenXml.Drawing;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Apps.DeepL;

public class DeepLInvocable : BaseInvocable
{
    protected Translator Client { get; }
    protected RestClient RestClient { get; }

    public DeepLInvocable(InvocationContext invocationContext) : base(invocationContext) 
    {
        Client = TranslatorFactory.GetTranslator(invocationContext.AuthenticationCredentialsProviders);
        var apiKey = InvocationContext.AuthenticationCredentialsProviders.First(p => p.KeyName == CredsNames.ApiKey).Value;
        RestClient = new RestClient("https://api.deepl.com/v2", configureSerialization: s => s.UseNewtonsoftJson());
        RestClient.AddDefaultHeader("Authorization", $"DeepL-Auth-Key {apiKey}");
    }

}