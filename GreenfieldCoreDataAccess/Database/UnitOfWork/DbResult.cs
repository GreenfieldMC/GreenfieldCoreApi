namespace GreenfieldCoreDataAccess.Database.UnitOfWork;

public class DbResult<T>
{
    
    public required T? Data { get; set; }
    public required bool IsSuccessful { get; set; }
    public required string? ErrorMessage { get; set; }
    
    public static DbResult<T> Success(T data) => new()
    {
        Data = data,
        IsSuccessful = true,
        ErrorMessage = null
    };
    
    public static DbResult<T> Failure(string errorMessage) => new()
    {
        Data = default,
        IsSuccessful = false,
        ErrorMessage = errorMessage
    };
    
    public void ThrowIfFailed()
    {
        if (!IsSuccessful)
        {
            throw new Exception(ErrorMessage);
        }
    }

    public T GetOrThrow(string? message = null)
    {
        return !IsSuccessful ? throw new Exception(message ?? ErrorMessage) : Data!;
    }
    
    public T GetOrThrow(Func<Exception> exceptionFactory) 
    {
        return !IsSuccessful ? throw exceptionFactory() : Data!;
    }
    
    public T GetOrDefault(T defaultValue)
    {
        return !IsSuccessful ? defaultValue : Data!;
    }
    
}