using Apps.DeepL.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using DeepL;
using DeepL.Model;

namespace Apps.DeepL
{
    [ActionList]
    public class Actions
    {
        [Action]
        public TextResult Translate(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, [ActionParameter] TextTranslationRequest request)
        {
            var authenticationCredentialsProvider = GetAuthenticationCredentialsProvider(authenticationCredentialsProviders);
            var translator = new Translator(authenticationCredentialsProvider.Value);
            return Task.Run(async () => await translator.TranslateTextAsync(request.Text, request.SourceLanguage, request.TargetLanguage)).Result;
        }

        private AuthenticationCredentialsProvider GetAuthenticationCredentialsProvider(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        {
            return authenticationCredentialsProviders.First(p => p.KeyName == "apiKey");
        }
    }
}