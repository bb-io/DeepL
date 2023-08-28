using Apps.DeepL.Factories;
using Apps.DeepL.Requests;
using Apps.DeepL.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using DeepL;

namespace Apps.DeepL
{
    [ActionList]
    public class Actions
    {
        [Action("Translate", Description = "Translate a text")]
        public async Task<TextResponse> Translate(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] TextTranslationRequest request)
        {
            var translator = TranslatorFactory.GetTranslator(authenticationCredentialsProviders);
            var result = await translator.TranslateTextAsync(request.Text, request.SourceLanguage,
                request.TargetLanguage, CreateTextTranslateOptions(request));
            return new TextResponse
            {
                TranslatedText = result.Text,
                DetectedSourceLanguage = result.DetectedSourceLanguageCode
            };
        }

        [Action("Translate document", Description = "Translate a document")]
        public async Task<FileResponse> TranslateDocument(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] DocumentTranslationRequest request)
        {
            var translator = TranslatorFactory.GetTranslator(authenticationCredentialsProviders);
            var stream = new MemoryStream(request.File.Bytes);
            var outputStream = new MemoryStream();
            await translator.TranslateDocumentAsync(stream, request.File.Name, outputStream, request.SourceLanguage,
                request.TargetLanguage, CreateDocumentTranslateOptions(request));
            
            return new()
            {
                File = new(outputStream.GetBuffer())
                {
                    Name = request.File.Name,
                    ContentType = request.File.ContentType
                }
            };
        }

        private TextTranslateOptions CreateTextTranslateOptions(TextTranslationRequest request)
        {
            var options = new TextTranslateOptions
            {
                PreserveFormatting = request.PreserveFormatting == null || (bool)request.PreserveFormatting,
                Formality = request.Formal == null
                    ? Formality.Default
                    : ((bool)request.Formal ? Formality.PreferMore : Formality.PreferLess),
                GlossaryId = request.GlossaryId,
                TagHandling = request.TagHandling,
                OutlineDetection = request.OutlineDetection == null || (bool)request.OutlineDetection,
            };

            if (request.NonSplittingTags != null)
                options.NonSplittingTags.AddRange(request.NonSplittingTags);

            if (request.SplittingTags != null)
                options.SplittingTags.AddRange(request.SplittingTags);

            if (request.IgnoreTags != null)
                options.IgnoreTags.AddRange(request.IgnoreTags);

            return options;
        }

        private DocumentTranslateOptions CreateDocumentTranslateOptions(DocumentTranslationRequest request)
        {
            return new DocumentTranslateOptions
            {
                Formality = request.Formal == null
                    ? Formality.Default
                    : ((bool)request.Formal ? Formality.PreferMore : Formality.PreferLess),
                GlossaryId = request.GlossaryId,
            };
        }
    }
}