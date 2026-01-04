using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories.Interfaces;

public interface IClientRepository
{
    /// <summary>
    /// Registers a new client and returns the generated ClientId (GUID).
    /// </summary>
    /// <param name="clientName"></param>
    /// <param name="clientSecretHash"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    Task<Result<(Guid, DateTime)>> RegisterClient(string clientName, string clientSecretHash, string salt);

    /// <summary>
    /// Verifies client credentials.
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="clientSecretHash"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    Task<Result<bool>> VerifyClientCredentials(Guid clientId, string clientSecretHash, string salt);

    /// <summary>
    /// Gets a client by their ID.
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    Task<Result<ClientEntity>> SelectClientById(Guid clientId);

    /// <summary>
    /// Gets a client by their name.
    /// </summary>
    /// <param name="clientName"></param>
    /// <returns></returns>
    Task<Result<ClientEntity>> SelectClientByName(string clientName);
    
    /// <summary>
    /// Gets all registered clients.
    /// </summary>
    /// <returns></returns>
    Task<Result<IEnumerable<ClientEntity>>> SelectClients();

    /// <summary>
    /// Deletes a client by their ID.
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    Task<Result> DeleteClient(Guid clientId);

    /// <summary>
    /// Gets all roles assigned to a client.
    /// </summary>
    /// <param name="clientId">Client who has roles</param>
    /// <returns>An enumerable of ClientRoles</returns>
    Task<Result<IEnumerable<ClientRoleEntity>>> SelectClientRoles(Guid clientId);

    /// <summary>
    /// Assigns a role to a client.
    /// </summary>
    /// <param name="clientId">Client to add the role to</param>
    /// <param name="roleName">Name of the role to add</param>
    /// <returns>True if the role was assigned, false otherwise</returns>
    Task<Result> AssignRoleToClient(Guid clientId, string roleName);

    /// <summary>
    /// Removes a role from a client.
    /// </summary>
    /// <param name="clientId">Client to remove the role from</param>
    /// <param name="roleName">Name of the role to remove</param>
    /// <returns>True if the role was removed, false otherwise</returns>
    Task<Result> RemoveRoleFromClient(Guid clientId, string roleName);
    
    /// <summary>
    /// Clears all roles assigned to a client.
    /// </summary>
    /// <param name="clientId">Client to clear roles from</param>
    /// <returns>The number of roles removed.</returns>
    Task<Result<int>> DeleteClientRoles(Guid clientId);

    /// <summary>
    /// Updates a client's name.
    /// </summary>
    /// <param name="clientId">The ID of the client to update.</param>
    /// <param name="newClientName">The new name for the client.</param>
    /// <returns>True if the client name was successfully updated, false otherwise.</returns>
    Task<Result> UpdateClientName(Guid clientId, string newClientName);

    /// <summary>
    /// Updates a client's secret hash and salt.
    /// </summary>
    /// <param name="clientId">The ID of the client to update.</param>
    /// <param name="newClientSecretHash">The new client secret hash.</param>
    /// <param name="newSalt">The new salt.</param>
    /// <returns></returns>
    Task<Result> UpdateClientSecret(Guid clientId, string newClientSecretHash, string newSalt);
    
}