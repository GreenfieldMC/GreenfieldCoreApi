using GreenfieldCoreServices.Models;
using GreenfieldCoreServices.Models.Connections.Patreon;
using GreenfieldCoreServices.Models.Users;

namespace GreenfieldCoreApi.ApiModels.Connections;

public record ApiPatreonConnection : IModelConvertable<PatreonConnection, ApiPatreonConnection>
{
    public required long PatreonConnectionId { get; init; }
    public required string FullName { get; init; }
    public required decimal? Pledge { get; init; }
    public required DateTime? UpdatedOn { get; init; }
    public required DateTime CreatedOn { get; init; }
    
    public static ApiPatreonConnection FromModel(PatreonConnection from)
    {
        return new ApiPatreonConnection
        {
            PatreonConnectionId = from.PatreonConnectionId,
            FullName = from.FullName,
            Pledge = from.Pledge,
            UpdatedOn = from.UpdatedOn,
            CreatedOn = from.CreatedOn
        };
    }
}

public record ApiPatreonConnectionWithUsers : ApiPatreonConnection
{
    public required IEnumerable<User> Users { get; init; }
}