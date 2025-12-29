using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories.Interfaces;

public interface IUserRepository
{
    /// <summary>
    /// Get a user by their internal user ID
    /// </summary>
    /// <param name="userId">The internal user ID of the user to retrieve</param>
    /// <returns>Result containing the UserEntity if found, or null if no user was found with the given ID.</returns>
    Task<Result<UserEntity?>> GetUserByUserId(long userId);
    
    /// <summary>
    /// Get a user by their Minecraft UUID
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to retrieve</param>
    /// <returns>Result containing the UserEntity if found, or null if no user was found with the given UUID.</returns>
    Task<Result<UserEntity?>> GetUserByUuid(Guid minecraftUuid);
    
    /// <summary>
    /// Create a new user. Returns Result containing the created UserEntity if an insert occurred, or null if no insert was performed.
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to create</param>
    /// <param name="minecraftUsername">The Minecraft username of the user to create</param>
    /// <returns>Result containing the created UserEntity, or null if no insert was performed.</returns>
    Task<Result<UserEntity?>> CreateUser(Guid minecraftUuid, string minecraftUsername);
    
    /// <summary>
    /// Update a user's Minecraft username. Returns Result true if an update was performed, false otherwise.
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to update</param>
    /// <param name="newMinecraftUsername">The new Minecraft username to set</param>
    /// <returns>Result true if an update occurred, false otherwise.</returns>
    Task<Result<bool>> UpdateUsername(Guid minecraftUuid, string newMinecraftUsername);

    /// <summary>
    /// Create a reference between a user and their Discord account.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="discordSnowflake">The Discord snowflake ID</param>
    /// <returns></returns>
    Task<Result<UserDiscordEntity?>> CreateUserDiscordReference(long userId, ulong discordSnowflake);

    /// <summary>
    /// Get all Discord references for a given user by their Minecraft UUID.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<Result<IEnumerable<UserDiscordEntity>>> GetUserDiscordReferences(long userId);

    /// <summary>
    /// Delete a reference between a user and their Discord account.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="discordSnowflake">The Discord snowflake ID</param>
    /// <returns></returns>
    Task<Result<bool>> DeleteUserDiscordReference(long userId, ulong discordSnowflake);
    
    /// <summary>
    /// Get all users linked to a Discord snowflake.
    /// </summary>
    /// <param name="discordSnowflake">The Discord snowflake ID.</param>
    /// <returns>Result containing the linked user entities (empty when none found).</returns>
    Task<Result<IEnumerable<UserEntity>>> GetUsersByDiscordSnowflake(ulong discordSnowflake);

}