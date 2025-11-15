namespace GreenfieldCoreServices.Models;

/// <summary>
/// Interface for models that can be converted from a database model to a domain model.
/// </summary>
/// <typeparam name="TFrom">The type of the database model.</typeparam>
/// <typeparam name="TSelf">The type of the domain model.</typeparam>
public interface IDbModelConvertable<in TFrom, out TSelf>
{
    
    static abstract TSelf FromDbModel(TFrom from);
    
}