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
        [Action("Translate", Description = "Translate a string")]
        public async Task<TextResult> Translate(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, [ActionParameter] TextTranslationRequest request)
        {
            var translator = CreateTranslator(authenticationCredentialsProviders);
            return await translator.TranslateTextAsync(request.Text, request.SourceLanguage, request.TargetLanguage);
        }

        private static Translator CreateTranslator(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        {
            var apiKey = authenticationCredentialsProviders.First(p => p.KeyName == "apiKey");
            return new Translator(apiKey.Value);
        }
    }
}