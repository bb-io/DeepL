using Apps.DeepL.Factories;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.DeepL.Connections;

public class ConnectionValidator : IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        var translator = TranslatorFactory.GetTranslator(authProviders);

        try
        {
            await translator.GetSourceLanguagesAsync(cancellationToken);

            return new()
            {
                IsValid = true
            };
        }
        catch (Exception ex)
        {
            return new()
            {
                IsValid = false,
                Message = ex.Message
            };
        }
    }
}