using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface IUserService
{
    
    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to create</param>
    /// <param name="username">The Minecraft username of the user to create</param>
    /// <returns>The created User if an insert occurred; a failed Result otherwise.</returns>
    public Task<Result<User>> CreateUser(Guid minecraftUuid, string username);
    
    /// <summary>
    /// Get a user by their Minecraft UUID
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to retrieve</param>
    /// <returns>The User if found; a failed Result if no user was found with the given UUID.</returns>
    public Task<Result<User>> GetUserByUuid(Guid minecraftUuid);
    
    /// <summary>
    /// Get a user by their internal user ID
    /// </summary>
    /// <param name="userId">The internal user ID of the user to retrieve</param>
    /// <returns>The User if found; a failed Result if no user was found with the given ID.</returns>
    public Task<Result<User>> GetUserByUserId(long userId);
    
    /// <summary>
    /// Updates a user's Minecraft username.
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to update</param>
    /// <param name="newUsername">The new Minecraft username to set</param>
    /// <returns>The updated User if an update was performed; a failed Result otherwise.</returns>
    public Task<Result<User>> UpdateUsername(Guid minecraftUuid, string newUsername);
    
    /// <summary>
    /// Links a Discord account to a user.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <param name="discordSnowflake">The Discord snowflake ID</param>
    /// <returns>True if the link was created; a failed Result otherwise.</returns>
    public Task<Result<bool>> LinkDiscordAccount(long userId, ulong discordSnowflake);
    
    /// <summary>
    /// Unlinks a Discord account from a user.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <param name="discordSnowflake">>The Discord snowflake ID</param>
    /// <returns>>True if the link was removed; a failed Result otherwise.</returns>
    public Task<Result<bool>> UnlinkDiscordAccount(long userId, ulong discordSnowflake);
    
    /// <summary>
    /// Gets all linked Discord accounts for a given user.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <returns>The list of linked Discord snowflake IDs (or an empty list of no accounts are linked). A failed Result otherwise.</returns>
    public Task<Result<IEnumerable<ulong>>> GetLinkedDiscordAccounts(long userId);
    
    /// <summary>
    /// Gets all linked Discord accounts for a given Minecraft UUID.
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID</param>
    /// <returns>>The list of linked Discord snowflake IDs (or an empty list of no accounts are linked). A failed Result otherwise.</returns>
    public Task<Result<IEnumerable<ulong>>> GetLinkedDiscordAccountsByUuid(Guid minecraftUuid);
    
}