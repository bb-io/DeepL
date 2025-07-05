using Apps.DeepL.Constants;
using Apps.DeepL.Models;
using Apps.DeepL.Requests;
using Apps.DeepL.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.DeepL.Actions;

[ActionList("Write")]
public class WriteActions(InvocationContext invocationContext) : DeepLInvocable(invocationContext)
{
    [Action("Improve text", Description = "Improve a text using DeepL Write")]
    public async Task<ImproveResponse> Improve([ActionParameter] ImproveRequest input)
    {
        var supportedLanguages = LanguageConstants.WriteLanguages.Keys;
        if (input.TargetLanguage != null && !supportedLanguages.Contains(input.TargetLanguage))
        {
            throw new PluginMisconfigurationException($"The target language '{input.TargetLanguage}' is not supported. Please select a valid language: {string.Join(", ", supportedLanguages)}.");
        }

        if (input.WritingStyle != null && input.Tone != null)
        {
            throw new PluginMisconfigurationException("Both writing style and tone are defined. DeepL does not allow this. Please select only either writing style or tone.");
        }

        if (string.IsNullOrEmpty(input.Text))
        {
            throw new PluginMisconfigurationException("The text can not be empty, please fill the 'Text' field and make sure it has content");
        }

        var request = new RestRequest("/write/rephrase", Method.Post);
        request.AddJsonBody(new
        {
            text = new[] { input.Text },
            target_lang = input.TargetLanguage,
            writing_style = input.WritingStyle,
            tone = input.Tone,
        });
        var result = await RestClient.ExecuteAsync<WriteResponseDto>(request);
        if (!result.IsSuccessful)
        {
            var dto = JsonConvert.DeserializeObject<ExceptionDto>(result.Content);
            throw new PluginApplicationException(dto.Message);
        }
        var response = result.Data.Improvements.First();
        return response;
    }
}
