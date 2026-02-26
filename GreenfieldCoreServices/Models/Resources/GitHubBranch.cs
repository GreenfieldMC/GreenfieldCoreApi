using System.Text.Json.Serialization;

namespace GreenfieldCoreServices.Models.Resources;

public record GitHubBranch(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("commit")] GitHubBranchCommit Commit
);

public record GitHubBranchCommit(
    [property: JsonPropertyName("sha")] string Sha
);

