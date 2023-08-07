using Blackbird.Applications.Sdk.Common.Authentication;
using DeepL;

namespace Apps.DeepL.Factories;

public static class TranslatorFactory
{
    public static Translator GetTranslator(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        var apiKey = creds.First(p => p.KeyName == "apiKey");
        return new(apiKey.Value, new()
        {
            Headers = new()
            {
                { "User-Agent", "Blackbird.io" }
            }
        });
    }
}