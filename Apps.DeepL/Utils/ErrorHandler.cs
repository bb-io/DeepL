namespace Apps.DeepL.Utils;

public static class ErrorHandler
{
    public static async Task ExecuteWithErrorHandlingAsync(Func<Task> action)
    {
        try
        { 
            await action();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error occurred while executing action: {ex.Message}");
        }
    }
    
    public static async Task<T> ExecuteWithErrorHandlingAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error occurred while executing action: {ex.Message}");
        }
    }
}