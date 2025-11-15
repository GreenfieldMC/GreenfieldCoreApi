using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Clients;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface IClientAuthService
{
    /// <summary>
    /// Registers a new client and returns a secret key.
    /// </summary>
    /// <param name="clientName">The name of the client to register</param>
    /// <param name="roles">Roles to assign to this user</param>
    /// <returns>A client and its secret</returns>
    Task<Result<(Client client, string secret)>> RegisterClient(string clientName, List<string> roles);

    /// <summary>
    /// Authenticates a client and returns a JWT token if successful.
    /// </summary>
    /// <param name="clientId">The ID of the client to authenticate</param>
    /// <param name="clientSecret">The secret of the client to authenticate</param>
    /// <returns>A JWT token. throws an exception if auth failed.</returns>
    Task<Result<string>> AuthenticateLogin(Guid clientId, string clientSecret);
    
    /// <summary>
    /// Gets all registered clients.
    /// </summary>
    /// <returns>An enumerable of registered clients.</returns>
    Task<Result<IEnumerable<Client>>> GetAllClients();

    /// <summary>
    /// Gets a client by their ID.
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns>The found client, or a failed result.</returns>
    Task<Result<Client>> GetClientById(Guid clientId);

    /// <summary>
    /// Gets a client by their name.
    /// </summary>
    /// <param name="clientName"></param>
    /// <returns>The found client, or a failed result</returns>
    Task<Result<Client>> GetClientByName(string clientName);

    /// <summary>
    /// Deletes a client by their ID.
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns>The deleted client, or a failed result.</returns>
    Task<Result<Client>> DeleteClient(Guid clientId);

    /// <summary>
    /// Updates the roles assigned to a client.
    /// </summary>
    /// <param name="clientId">The ID of the client to update.</param>
    /// <param name="roles">The list of roles to assign to the client.</param>
    /// <returns>THe updated client, or a failed result.</returns>
    Task<Result<Client>> UpdateClientRoles(Guid clientId, List<string> roles);

    /// <summary>
    /// Refreshes a client's secret and returns the new secret.
    /// </summary>
    /// <param name="clientId">The ID of the client to refresh the secret for.</param>
    /// <returns>The new client secret, or a failed result if the client was not found.</returns>
    Task<Result<string>> RefreshClientSecret(Guid clientId);

    /// <summary>
    /// Updates a client's name.
    /// </summary>
    /// <param name="clientId">The ID of the client to update.</param>
    /// <param name="newName">The new name for the client.</param>
    /// <returns>The updated client, or a failed result if the client was not found or could not be updated.</returns>
    Task<Result<Client>> UpdateClientName(Guid clientId, string newName);

    /// <summary>
    /// Clears all roles assigned to a client.
    /// </summary>
    /// <param name="clientId">The ID of the client to clear roles for.</param>
    /// <returns>The updated client, or a failed result.</returns>
    Task<Result<Client>> ClearClientRoles(Guid clientId);
}