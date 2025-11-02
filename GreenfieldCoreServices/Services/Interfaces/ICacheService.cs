namespace GreenfieldCoreServices.Services.Interfaces;

public interface ICacheService<TKey, TValue>
{
    bool TryGetValue(TKey key, out TValue? value);
    bool TryGetValue(Func<TValue, bool> predicate, out TValue? value);
    void SetValue(TKey key, TValue value);
    void RemoveValue(TKey key);
    
    void ClearCache();
}