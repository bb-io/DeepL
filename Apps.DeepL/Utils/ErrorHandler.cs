using Blackbird.Applications.Sdk.Common.Exceptions;
using DeepL;

namespace Apps.DeepL.Utils;

public static class ErrorHandler
{
    public static async Task ExecuteWithErrorHandlingAsync(Func<Task> action)
    {
        try
        { 
            await action();
        }
        catch (AuthorizationException)
        {
            throw new PluginMisconfigurationException("Your DeepL API credentials are invalid. Please check your credentials or account status.");
        }
        catch (DeepLException ex)
        {
            throw new PluginApplicationException(ex.Message);
        }
    }
    
    public static async Task<T> ExecuteWithErrorHandlingAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (AuthorizationException)
        {
            throw new PluginMisconfigurationException("Your DeepL API credentials are invalid. Please check your credentials or account status.");
        }
        catch (DeepLException ex)
        {
            throw new PluginApplicationException(ex.Message);
        }
    }
}