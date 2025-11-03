using GreenfieldCoreServices.Models;
using GreenfieldCoreServices.Models.Users;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface IUserService
{
    
    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to create</param>
    /// <param name="username">The Minecraft username of the user to create</param>
    /// <returns>The created User if an insert occurred; null if the user already exists or could not be created.</returns>
    public Task<User?> CreateUser(Guid minecraftUuid, string username);
    
    /// <summary>
    /// Get a user by their Minecraft UUID
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to retrieve</param>
    /// <returns>The User if found, or null if no user was found with the given UUID.</returns>
    public Task<User?> GetUserByUuid(Guid minecraftUuid);
    
    /// <summary>
    /// Get a user by their internal user ID
    /// </summary>
    /// <param name="userId">The internal user ID of the user to retrieve</param>
    /// <returns>The User if found, or null if no user was found with the given ID.</returns>
    public Task<User?> GetUserByUserId(long userId);
    
    /// <summary>
    /// Updates a user's Minecraft username.
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to update</param>
    /// <param name="newUsername">The new Minecraft username to set</param>
    /// <returns>The updated User if an update was performed, or null otherwise.</returns>
    public Task<User?> UpdateUsername(Guid minecraftUuid, string newUsername);

}