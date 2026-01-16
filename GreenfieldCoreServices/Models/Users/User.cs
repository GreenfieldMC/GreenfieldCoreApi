using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.Users;

public class User : IModelConvertable<UserEntity, User>
{
    
    /// <summary>
    /// The id used in the database
    /// </summary>
    public required long UserId { get; set; }
   
    /// <summary>
    /// This user's minecraft account uuid
    /// </summary>
    public required Guid MinecraftUuid { get; set; }
    
    /// <summary>
    /// This user's last known minecraft account username
    /// </summary>
    public required string Username { get; set; }
    
    /// <summary>
    /// The date and time this user was created
    /// </summary>
    public required DateTime CreatedOn { get; set; }

    public static User FromModel(UserEntity from)
    {
        return new User
        {
            UserId = from.UserId,
            MinecraftUuid = from.MinecraftUuid,
            Username = from.MinecraftUsername,
            CreatedOn = from.CreatedOn
        };
    }
}