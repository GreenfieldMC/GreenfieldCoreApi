namespace GreenfieldCoreServices.Models.Resources;

public record DownloadToken(Guid TokenId, string BranchName, DateTime CreatedAt);

