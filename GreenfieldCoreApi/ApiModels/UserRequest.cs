namespace GreenfieldCoreApi.ApiModels;

public class UserRequest
{
    public required Guid MinecraftUuid { get; set; }
    public required string Username { get; init; }
}
