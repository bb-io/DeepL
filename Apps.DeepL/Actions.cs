using Apps.DeepL.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using DeepL;
using DeepL.Model;

namespace Apps.DeepL
{
    [ActionList]
    public class Actions
    {
        [Action]
        public TextResult Translate(AuthenticationCredentialsProvider authenticationCredentialsProvider, [ActionParameter] TextTranslationRequest request)
        {
            var translator = new Translator(authenticationCredentialsProvider.Value);
            return Task.Run(async () => await translator.TranslateTextAsync(request.Text, request.SourceLanguage, request.TargetLanguage)).Result;
        }
    }
}