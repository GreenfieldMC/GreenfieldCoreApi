using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreDataAccess.Database.Repositories.Interfaces;

public interface IUserRepository
{
    
    /// <summary>
    /// Get a user by their internal user ID
    /// </summary>
    /// <param name="userId">The internal user ID of the user to retrieve</param>
    /// <returns>The UserEntity if found, or null if no user was found with the given ID.</returns>
    Task<UserEntity?> GetUserByUserId(long userId);
    
    /// <summary>
    /// Get a user by their Minecraft UUID
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to retrieve</param>
    /// <returns></returns>
    Task<UserEntity?> GetUserByUuid(Guid minecraftUuid);
    
    /// <summary>
    /// Create a new user. Returns the created UserEntity if an insert occurred, or null if no insert was performed.
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to create</param>
    /// <param name="minecraftUsername">The Minecraft username of the user to create</param>
    /// <returns>The created UserEntity, or null if no insert was performed.</returns>
    Task<UserEntity?> CreateUser(Guid minecraftUuid, string minecraftUsername);
    
    /// <summary>
    /// Update a user's Minecraft username. Returns true if an update was performed, false otherwise.
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to update</param>
    /// <param name="newMinecraftUsername">The new Minecraft username to set</param>
    /// <returns>True if an update occurred, false otherwise.</returns>
    Task<bool> UpdateUsername(Guid minecraftUuid, string newMinecraftUsername);
    
}