using System.Data;
using Dapper;

namespace GreenfieldCoreDataAccess.Database.Procedures;

public static class DbConnectionExtensions
{

    /// <summary>
    /// Executes a stored procedure that does not return any result set.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="procedure"></param>
    /// <param name="transaction"></param>
    /// <returns>The number of rows affected.</returns>
    public static Task<int> ExecuteProcedure(this IDbConnection connection, Procedure procedure, IDbTransaction? transaction)
    {
        return connection.ExecuteAsync(procedure.GetProcedureName(), commandType: CommandType.StoredProcedure, transaction: transaction);
    }

    /// <summary>
    /// Executes a stored procedure that does not return any result set, with parameters.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="procedure"></param>
    /// <param name="parameters"></param>
    /// <param name="transaction"></param>
    /// <typeparam name="TParams"></typeparam>
    /// <returns>The number of rows affected.</returns>
    public static Task<int> ExecuteProcedure<TParams>(this IDbConnection connection, ParameterizedProcedure<TParams> procedure, TParams parameters, IDbTransaction? transaction)
    {
        return connection.ExecuteAsync(procedure.GetProcedureName(), procedure.ResolveParameters(parameters), commandType: CommandType.StoredProcedure, transaction: transaction);
    }
    
    /// <summary>
    /// Executes a stored procedure that returns a single scalar value. Useful for COUNT, SUM, etc.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="procedure"></param>
    /// <param name="transaction"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>The scalar result.</returns>
    public static Task<T?> ExecuteScalarProcedure<T>(this IDbConnection connection, QuerySingleProcedure<T> procedure, IDbTransaction? transaction)
    {
        return connection.ExecuteScalarAsync<T?>(procedure.GetProcedureName(), commandType: CommandType.StoredProcedure, transaction: transaction);
    }
    
    /// <summary>
    /// Executes a stored procedure that returns a single scalar value, with parameters. Useful for COUNT, SUM, etc.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="procedure"></param>
    /// <param name="parameters"></param>
    /// <param name="transaction"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TParams"></typeparam>
    /// <returns>>The scalar result.</returns>
    public static Task<T?> ExecuteScalarProcedure<T, TParams>(this IDbConnection connection, ParameterizedQuerySingleProcedure<T, TParams> procedure, TParams parameters, IDbTransaction? transaction)
    {
        return connection.ExecuteScalarAsync<T?>(procedure.GetProcedureName(), procedure.ResolveParameters(parameters), commandType: CommandType.StoredProcedure, transaction: transaction);
    }
    
    /// <summary>
    /// Executes a stored procedure that returns a single row result.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="procedure"></param>
    /// <param name="transaction"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>>The single result. Null if not found.</returns>
    public static Task<T?> QuerySingleProcedure<T>(this IDbConnection connection, QuerySingleProcedure<T> procedure, IDbTransaction? transaction)
    {
        return connection.QuerySingleOrDefaultAsync<T?>(procedure.GetProcedureName(), commandType: CommandType.StoredProcedure, transaction: transaction);
    }
    
    /// <summary>
    /// Executes a stored procedure that returns a single row result, with parameters.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="procedure"></param>
    /// <param name="parameters"></param>
    /// <param name="transaction"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TParams"></typeparam>
    /// <returns>>The single result. Null if not found.</returns>
    public static Task<T?> QuerySingleProcedure<T, TParams>(this IDbConnection connection, ParameterizedQuerySingleProcedure<T, TParams> procedure, TParams parameters, IDbTransaction? transaction)
    {
        return connection.QuerySingleOrDefaultAsync<T?>(procedure.GetProcedureName(), procedure.ResolveParameters(parameters), commandType: CommandType.StoredProcedure, transaction: transaction);
    }
    
    /// <summary>
    /// Executes a stored procedure that returns multiple rows.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="procedure"></param>
    /// <param name="transaction"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>>The result set.</returns>
    public static Task<IEnumerable<T>> QueryProcedure<T>(this IDbConnection connection, QueryProcedure<T> procedure, IDbTransaction? transaction)
    {
        return connection.QueryAsync<T>(procedure.GetProcedureName(), commandType: CommandType.StoredProcedure, transaction: transaction);
    }
    
    /// <summary>
    /// Executes a stored procedure that returns multiple rows, with parameters.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="procedure"></param>
    /// <param name="parameters"></param>
    /// <param name="transaction"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TParams"></typeparam>
    /// <returns>>The result set.</returns>
    public static Task<IEnumerable<T>> QueryProcedure<T, TParams>(this IDbConnection connection, ParameterizedQueryProcedure<T, TParams> procedure, TParams parameters, IDbTransaction? transaction)
    {
        return connection.QueryAsync<T>(procedure.GetProcedureName(), procedure.ResolveParameters(parameters), commandType: CommandType.StoredProcedure, transaction: transaction);
    }

}