using System.Diagnostics.CodeAnalysis;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface ICacheService<TKey, TValue>
{
    /// <summary>
    /// Tries to get a value from the cache by key.
    /// </summary>
    /// <param name="key">The key to look for.</param>
    /// <param name="value">The value associated with the key, or null if not found.</param>
    /// <returns>>True if the key was found; otherwise, false.</returns>
    bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value);
    
    /// <summary>
    /// Tries to get a value from the cache by predicate.
    /// </summary>
    /// <param name="predicate">The predicate to match.</param>
    /// <param name="value">The value that matches the predicate, or null if not found.</param>
    /// <returns>>True if a matching value was found; otherwise, false.</returns>
    bool TryGetValue(Func<TValue, bool> predicate, [MaybeNullWhen(false)] out TValue value);
    
    /// <summary>
    /// Get a copy of the entire cache dictionary.
    /// </summary>
    /// <returns>>The dictionary representing the cache.</returns>
    IDictionary<TKey, TValue> GetDictionary();
    
    /// <summary>
    /// Gets all keys in the cache.
    /// </summary>
    /// <returns></returns>
    IEnumerable<TKey> GetKeys();
    
    /// <summary>
    /// Gets all values in the cache.
    /// </summary>
    /// <returns></returns>
    IEnumerable<TValue> GetValues();
    
    /// <summary>
    /// Gets the count of items in the cache.
    /// </summary>
    /// <returns></returns>
    long GetCount();
    
    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    /// <param name="key">The key to set.</param>
    /// <param name="value">The value to set.</param>
    void SetValue(TKey key, TValue value);
    
    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    void RemoveValue(TKey key);
    
    /// <summary>
    /// Clears the entire cache.
    /// </summary>
    void ClearCache();
}