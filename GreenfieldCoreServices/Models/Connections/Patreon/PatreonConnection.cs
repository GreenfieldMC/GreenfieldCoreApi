using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.Connections.Patreon;

public record PatreonConnection : IModelConvertable<PatreonConnectionEntity, PatreonConnection>
{
    
    public required long PatreonConnectionId { get; init; }
    public required string RefreshToken { get; init; }
    public required string AccessToken { get; init; }
    public required DateTime RefreshBy { get; init; }
    public required DateTime? UpdatedOn { get; init; }
    public required DateTime CreatedOn { get; init; }
    
    public required long PatreonId { get; init; }
    public required string FullName { get; init; }
    public required decimal? Pledge { get; init; }
    
    public static PatreonConnection FromModel(PatreonConnectionEntity from)
    {
        return new PatreonConnection
        {
            PatreonConnectionId = from.PatreonConnectionId,
            RefreshToken = from.RefreshToken,
            AccessToken = from.AccessToken,
            RefreshBy = from.TokenExpiry,
            UpdatedOn = from.UpdatedOn,
            CreatedOn = from.CreatedOn,
            PatreonId = from.PatreonId,
            FullName = from.FullName,
            Pledge = from.Pledge
        };
    }
}

public record UserPatreonConnection : IModelConvertable<UserPatreonConnectionEntity, UserPatreonConnection>
{
    public required long UserPatreonConnectionId { get; init; }
    public required long UserId { get; init; }
    public required long PatreonConnectionId { get; init; }
    public required DateTime ConnectedOn { get; init; }

    public static UserPatreonConnection FromModel(UserPatreonConnectionEntity from)
    {
        return new UserPatreonConnection
        {
            UserPatreonConnectionId = from.UserPatreonConnectionId,
            UserId = from.UserId,
            PatreonConnectionId = from.PatreonConnectionId,
            ConnectedOn = from.UserPatreonConnectionCreatedOn
        };
    }
}