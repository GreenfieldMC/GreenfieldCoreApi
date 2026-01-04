using Dapper;

namespace GreenfieldCoreDataAccess.Database.Procedures;

/// <summary>
/// Represents a stored procedure without parameters and no returned data.
/// </summary>
public class Procedure(string procedureName)
{
    /// <summary>
    /// Gets the name of the stored procedure.
    /// </summary>
    /// <returns></returns>
    public string GetProcedureName() => procedureName;
}

/// <summary>
/// Represents a stored procedure with parameters and no returned data.
/// </summary>
/// <typeparam name="TParams"></typeparam>
public class ParameterizedProcedure<TParams>(string procedureName, Func<TParams, DynamicParameters, DynamicParameters> parameters) : Procedure(procedureName)
{
    /// <summary>
    /// Resolves the parameters for the procedure call.
    /// </summary>
    /// <param name="passedParams"></param>
    /// <returns></returns>
    public DynamicParameters ResolveParameters(TParams passedParams) => parameters.Invoke(passedParams, new DynamicParameters());
}

/// <summary>
/// Represents a stored procedure that returns a single scalar value.
/// </summary>
/// <param name="procedureName"></param>
/// <typeparam name="T"></typeparam>
public class QuerySingleProcedure<T>(string procedureName) : Procedure(procedureName);

/// <summary>
/// Represents a stored procedure with parameters that returns a single scalar value.
/// </summary>
/// <param name="procedureName"></param>
/// <param name="parameters"></param>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TParams"></typeparam>
public class ParameterizedQuerySingleProcedure<T, TParams>(string procedureName, Func<TParams, DynamicParameters, DynamicParameters> parameters) : ParameterizedProcedure<TParams>(procedureName, parameters);

/// <summary>
/// Represents a stored procedure that returns a collection of data.
/// </summary>
/// <param name="procedureName"></param>
/// <typeparam name="T"></typeparam>
public class QueryProcedure<T>(string procedureName) : Procedure(procedureName);

/// <summary>
/// Represents a stored procedure with parameters that returns a collection of data.
/// </summary>
/// <param name="procedureName"></param>
/// <param name="parameters"></param>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TParams"></typeparam>
public class ParameterizedQueryProcedure<T, TParams>(string procedureName, Func<TParams, DynamicParameters, DynamicParameters> parameters) : ParameterizedProcedure<TParams>(procedureName, parameters);