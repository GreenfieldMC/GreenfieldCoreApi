namespace GreenfieldCoreServices.Models.Resources;

public record ResourcePackCacheEntry(string Branch, string CommitHash, byte[] ZipBytes, DateTime CachedAt);

