using System.Data.Common;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Procedures;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class ClientRepository(IUnitOfWork uow, ILogger<IClientRepository> logger) : BaseRepository(uow), IClientRepository
{
    
    /// <inheritdoc />
    public async Task<Result<(Guid, DateTime)>> RegisterClient(string clientName, string clientSecretHash, string salt)
    {
        try
        {
            var guid = Guid.NewGuid();
            var createdOn = await Connection.ExecuteScalarProcedure(StoredProcs.Clients.RegisterClient, (guid, clientName, clientSecretHash, salt), Transaction);
            return Result<(Guid, DateTime)>.Success((guid, createdOn));
        }
        catch (DbException e)
        {
            logger.LogDebug("{ErrorMessage}", e.Message);
            return Result<(Guid, DateTime)>.Failure($"Failed to register client: {e.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> VerifyClientCredentials(Guid clientId, string clientSecretHash, string salt)
    {
        try
        {
            var isValid = await Connection.ExecuteScalarProcedure(StoredProcs.Clients.VerifyClientCredentials, (clientId, clientSecretHash, salt), Transaction);
            return Result<bool>.Success(isValid);
        }
        catch (DbException e)
        {
            logger.LogDebug("{ErrorMessage}", e.Message);
            return Result<bool>.Failure($"Failed to verify client credentials: {e.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<ClientEntity>> SelectClientById(Guid clientId)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Clients.SelectClientById, clientId, Transaction);
            return result is null
                ? Result<ClientEntity>.Failure($"Client {clientId} not found.")
                : Result<ClientEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<ClientEntity>.Failure($"Failed to get client by ID: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<ClientEntity>> SelectClientByName(string clientName)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Clients.SelectClientByName, clientName, Transaction);
            return result is null 
                ? Result<ClientEntity>.Failure($"Client {clientName} not found.") 
                : Result<ClientEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<ClientEntity>.Failure($"Failed to get client by name: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<ClientEntity>>> SelectClients()
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Clients.SelectAllClients, Transaction);
            return Result<IEnumerable<ClientEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<ClientEntity>>.Failure($"Failed to get clients: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteClient(Guid clientId)
    {
        try {
            var affected = await Connection.ExecuteProcedure(StoredProcs.Clients.DeleteClient, clientId, Transaction);
            return affected > 0
                ? Result.Success()
                : Result.Failure("No client was deleted.");
        } catch (DbException ex) {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to delete client: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<ClientRoleEntity>>> SelectClientRoles(Guid clientId)
    {
        try {
            var result = await Connection.QueryProcedure(StoredProcs.Clients.SelectClientRoles, clientId, Transaction);
            return Result<IEnumerable<ClientRoleEntity>>.Success(result);
        } catch (DbException ex) {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<ClientRoleEntity>>.Failure($"Failed to get client roles: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> AssignRoleToClient(Guid clientId, string roleName)
    {
        try
        {
            var affected = await Connection.ExecuteProcedure(StoredProcs.Clients.InsertClientRole, (clientId, roleName), Transaction);
            return affected > 0
                ? Result.Success()
                : Result.Failure("No role was assigned to the client.");
        } catch (DbException ex) {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to assign role to client: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> RemoveRoleFromClient(Guid clientId, string roleName)
    {
        try {
            var affected = await Connection.ExecuteProcedure(StoredProcs.Clients.DeleteClientRole, (clientId, roleName), Transaction);
            return affected > 0
                ? Result.Success()
                : Result.Failure("No role was removed from the client.");
        } catch (DbException ex) {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to remove role from client: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<int>> DeleteClientRoles(Guid clientId)
    {
        try {
            var affected = await Connection.ExecuteProcedure(StoredProcs.Clients.ClearClientRoles, clientId, Transaction);
            return Result<int>.Success(affected);
        } catch (DbException ex) {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<int>.Failure($"Failed to clear client roles: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> UpdateClientName(Guid clientId, string newClientName)
    {
        try
        {
            var affected = await Connection.ExecuteProcedure(StoredProcs.Clients.UpdateClientName, (clientId, newClientName), Transaction);
            return affected > 0
                ? Result.Success()
                : Result.Failure("No client name was updated.");
        } catch (DbException ex) {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to update client name: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> UpdateClientSecret(Guid clientId, string newClientSecretHash, string newSalt)
    {
        try
        {
            var affected = await Connection.ExecuteProcedure(StoredProcs.Clients.UpdateClientSecret, (clientId, newClientSecretHash, newSalt), Transaction);
            return affected > 0
                ? Result.Success()
                : Result.Failure("No client secret was updated.");
        } catch (DbException ex) {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to update client secret: {ex.Message}");
        }
    }
}