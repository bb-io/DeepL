using Apps.DeepL.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using DeepL;

namespace Apps.DeepL.Factories;

public static class TranslatorFactory
{
    public static Translator GetTranslator(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        var apiKey = creds.First(p => p.KeyName == CredsNames.ApiKey);
        return new(apiKey.Value, new()
        {
            appInfo = new AppInfo { AppName = "Blackbird.io", AppVersion = "1.1.x" }
        });
    }
}