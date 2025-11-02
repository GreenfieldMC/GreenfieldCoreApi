namespace GreenfieldCoreServices.Models;

public interface IDbModelConvertable<TFrom, TSelf>
{
    
    static abstract TSelf FromDbModel(TFrom from);
    
}