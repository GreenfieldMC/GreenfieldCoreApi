using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using GreenfieldCoreServices.Services.Interfaces;

namespace GreenfieldCoreServices.Services.Caching;

public class BaseCacheService<TKey, TValue> : ICacheService<TKey, TValue> where TKey : notnull
{
    
    private readonly IDictionary<TKey, TValue> _cache = new ConcurrentDictionary<TKey, TValue>();
    
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _cache.TryGetValue(key, out value);

    public bool TryGetValue(Func<TValue, bool> predicate, [MaybeNullWhen(false)] out TValue value) => _cache.Values.FirstOrDefault(predicate) is { } foundValue
        ? (value = foundValue) != null
        : (value = default!) != null;

    public IDictionary<TKey, TValue> GetDictionary() => _cache.ToImmutableDictionary();

    public IEnumerable<TKey> GetKeys() => _cache.Keys.ToImmutableList();

    public IEnumerable<TValue> GetValues() => _cache.Values.ToImmutableList();

    public long GetCount() => _cache.Count;

    public void SetValue(TKey key, TValue value) => _cache[key] = value;

    public void RemoveValue(TKey key) => _cache.Remove(key);
    
    public void ClearCache() => _cache.Clear();
}