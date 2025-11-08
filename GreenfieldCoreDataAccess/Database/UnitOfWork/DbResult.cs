using System.Diagnostics.CodeAnalysis;

namespace GreenfieldCoreDataAccess.Database.UnitOfWork;

public class DbResult<T>
{
    
    /// <summary>
    /// The data returned from the database operation. Default if the operation failed.
    /// </summary>
    public required T? Data { get; set; }
    
    /// <summary>
    /// Indicates whether the database operation was successful. Under normal usage, a failed operation is one that throws a DbException.
    /// </summary>
    public required bool IsSuccessful { get; set; }
    
    /// <summary>
    /// The error message if the operation failed. Null if the operation was successful.
    /// </summary>
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

    /// <summary>
    /// If the Database operation did not succeed, throw an exception with the provided message or the error message from the DbResult.
    /// <br/>
    /// Note: If the operation did not throw a DbException, the Data property may still be null even if IsSuccessful is true.
    /// </summary>
    /// <param name="message">Optional custom message for the exception.</param>
    /// <returns></returns>
    /// <exception cref="Exception">Thrown if the operation was not successful.</exception>
    public T? GetOrThrow(string? message = null)
    {
        return !IsSuccessful ? throw new Exception(message ?? ErrorMessage) : Data;
    }

    /// <summary>
    /// If the Database operation did not succeed, throw an exception with the provided unsuccessful message or the error message from the DbResult.
    /// If the operation was successful but the Data is null, throw an exception with the provided null data message or a default message.
    /// <br/>
    /// Note: This should never return null.
    /// </summary>
    /// <param name="unsuccessfulMessage">The message to use if the operation was not successful.</param>
    /// <param name="nullDataMessage">The message to use if the operation was successful but the Data is null.</param>
    /// <returns></returns>
    /// <exception cref="Exception">Thrown if the operation was not successful or if the Data is null.</exception>
    [return: NotNull]
    public T GetNonNullOrThrow(string? unsuccessfulMessage = null, string? nullDataMessage = null)
    {
        return !IsSuccessful
            ? throw new Exception(unsuccessfulMessage ?? ErrorMessage)
            : Data ?? throw new Exception(nullDataMessage ?? "Database operation returned null data.");
    }
    
    /// <summary>
    /// If the Database operation did not succeed, throw an exception created by the provided factory function.
    /// <br/>
    /// Note: If the operation did not throw a DbException, the Data property may still be null even if IsSuccessful is true.
    /// </summary>
    /// <param name="exceptionFactory">The factory function to create the exception to throw.</param>
    /// <returns></returns>
    /// <exception cref="Exception">>Thrown if the operation was not successful.</exception>
    public T? GetOrThrow(Func<Exception> exceptionFactory) 
    {
        return !IsSuccessful ? throw exceptionFactory() : Data;
    }
    
    /// <summary>
    /// If the Database operation did not succeed, return the provided default value.
    /// <br/>
    /// Note: If the operation did not throw a DbException, the Data property may still be null even if IsSuccessful is true.
    /// </summary>
    /// <param name="defaultValue">The default value to return if the operation was not successful.</param>
    /// <returns></returns>
    public T? GetOrDefault(T? defaultValue = default)
    {
        return !IsSuccessful ? defaultValue : Data;
    }
    
}