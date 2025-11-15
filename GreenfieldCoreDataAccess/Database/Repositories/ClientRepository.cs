using System.Data;
using System.Data.Common;
using Dapper;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class ClientRepository(IUnitOfWork uow) : BaseRepository(uow), IClientRepository
{
    
    private const string DeleteClientProc = "usp_DeleteClient";
    private const string GetAllClientsProc = "usp_GetAllClients";
    private const string GetClientByIdProc = "usp_GetClientById";
    private const string GetClientByNameProc = "usp_GetClientByName";
    private const string RegisterClientProc = "usp_RegisterClient";
    private const string UpdateClientNameProc = "usp_UpdateClientName";
    private const string UpdateClientSecretProc = "usp_UpdateClientSecret";
    private const string VerifyClientCredentialsProc = "usp_VerifyClient";
    
    private const string ClearClientRolesProc = "usp_ClearClientRoles";
    private const string DeleteClientRoleProc = "usp_DeleteClientRole";
    private const string InsertClientRoleProc = "usp_InsertClientRole";
    private const string SelectClientRolesProc = "usp_SelectClientRoles";
    

    /// <inheritdoc />
    public async Task<Result<(Guid, DateTime)>> RegisterClient(string clientName, string clientSecretHash,
        string salt)
    {
        var guid = Guid.NewGuid();
        
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", guid, DbType.Guid);
        parameters.Add("p_ClientName", clientName, DbType.String, size: 255);
        parameters.Add("p_ClientSecretHash", clientSecretHash, DbType.String, size: 255);
        parameters.Add("p_Salt", salt, DbType.String, size: 255);
        
        try 
        {
            var createdOn = await Connection.ExecuteScalarAsync<DateTime>(RegisterClientProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<(Guid, DateTime)>.Success((guid, createdOn));
        }
        catch (DbException ex)
        {
            return Result<(Guid, DateTime)>.Failure($"Failed to register client: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> VerifyClientCredentials(Guid clientId, string clientSecretHash,
        string salt)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        parameters.Add("p_ClientSecretHash", clientSecretHash, DbType.String, size: 255);
        parameters.Add("p_Salt", salt, DbType.String, size: 255);

        try
        {
            return Result<bool>.Success(await Connection.ExecuteScalarAsync<bool>(VerifyClientCredentialsProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction));
        }
        catch (DbException e)
        {
            return Result<bool>.Failure($"Failed to verify client credentials: {e.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<ClientEntity?>> GetClientById(Guid clientId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        try {
            var result = await Connection.QuerySingleOrDefaultAsync<ClientEntity?>(GetClientByIdProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<ClientEntity?>.Success(result);
        } catch (DbException ex) {
            return Result<ClientEntity?>.Failure($"Failed to get client by ID: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<ClientEntity?>> GetClientByName(string clientName)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientName", clientName, DbType.String, size: 255);
        try {
            var result = await Connection.QuerySingleOrDefaultAsync<ClientEntity?>(GetClientByNameProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<ClientEntity?>.Success(result);
        } catch (DbException ex) {
            return Result<ClientEntity?>.Failure($"Failed to get client by name: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<ClientEntity>>> GetAllClients()
    {
        try {
            var result = await Connection.QueryAsync<ClientEntity>(GetAllClientsProc, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<IEnumerable<ClientEntity>>.Success(result);
        } catch (DbException ex) {
            return Result<IEnumerable<ClientEntity>>.Failure($"Failed to get all clients: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> DeleteClient(Guid clientId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        try {
            var affected = await Connection.ExecuteAsync(DeleteClientProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<bool>.Success(affected > 0);
        } catch (DbException ex) {
            return Result<bool>.Failure($"Failed to delete client: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<ClientRoleEntity>>> GetClientRoles(Guid clientId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        try {
            var result = await Connection.QueryAsync<ClientRoleEntity>(SelectClientRolesProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<IEnumerable<ClientRoleEntity>>.Success(result);
        } catch (DbException ex) {
            return Result<IEnumerable<ClientRoleEntity>>.Failure($"Failed to get client roles: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> AssignRoleToClient(Guid clientId, string roleName)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        parameters.Add("p_RoleName", roleName, DbType.String, size: 255);
        try {
            var affected = await Connection.ExecuteAsync(InsertClientRoleProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<bool>.Success(affected > 0);
        } catch (DbException ex) {
            return Result<bool>.Failure($"Failed to assign role to client: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> RemoveRoleFromClient(Guid clientId, string roleName)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        parameters.Add("p_RoleName", roleName, DbType.String, size: 255);
        try {
            var affected = await Connection.ExecuteAsync(DeleteClientRoleProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<bool>.Success(affected > 0);
        } catch (DbException ex) {
            return Result<bool>.Failure($"Failed to remove role from client: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<int>> ClearClientRoles(Guid clientId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        try {
            var affected = await Connection.ExecuteAsync(ClearClientRolesProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<int>.Success(affected);
        } catch (DbException ex) {
            return Result<int>.Failure($"Failed to clear client roles: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> UpdateClientName(Guid clientId, string newClientName)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        parameters.Add("p_NewClientName", newClientName, DbType.String, size: 255);
        try {
            var affected = await Connection.ExecuteAsync(UpdateClientNameProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<bool>.Success(affected > 0);
        } catch (DbException ex) {
            return Result<bool>.Failure($"Failed to update client name: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> UpdateClientSecret(Guid clientId, string newClientSecretHash, string newSalt)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        parameters.Add("p_NewSecretHash", newClientSecretHash, DbType.String, size: 255);
        parameters.Add("p_NewSalt", newSalt, DbType.String, size: 255);
        try {
            var affected = await Connection.ExecuteAsync(UpdateClientSecretProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<bool>.Success(affected > 0);
        } catch (DbException ex) {
            return Result<bool>.Failure($"Failed to update client secret: {ex.Message}");
        }
    }
}