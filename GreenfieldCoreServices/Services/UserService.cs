using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.Interfaces;

namespace GreenfieldCoreServices.Services;

public class UserService(IUnitOfWork uow) : IUserService
{
    public async Task<User?> CreateUser(Guid minecraftUuid, string username)
    {
        var repo = uow.Repository<IUserRepository>();
        
        var foundUser = await repo.GetUserByUuid(minecraftUuid);
        if (foundUser is not null) return null;
        
        uow.BeginTransaction();
        var created = await repo.CreateUser(minecraftUuid, username);
        if (created is null) return null;
        uow.CompleteAndCommit();

        return User.FromDbModel(created);
    }

    public async Task<User?> GetUserByUuid(Guid minecraftUuid)
    {
        var repo = uow.Repository<IUserRepository>();
        var foundUser = await repo.GetUserByUuid(minecraftUuid);
        
        return foundUser is null ? null : User.FromDbModel(foundUser);
    }

    public Task<User?> GetUserByUserId(long userId)
    {
        var repo = uow.Repository<IUserRepository>();
        return repo.GetUserByUserId(userId).ContinueWith(t =>
            t.Result is null ? null : User.FromDbModel(t.Result));
    }

    public async Task<User?> UpdateUsername(Guid minecraftUuid, string newUsername)
    {
        var repo = uow.Repository<IUserRepository>();
        uow.BeginTransaction();
        
        if (!await repo.UpdateUsername(minecraftUuid, newUsername)) return null;
        
        var updatedUser = await repo.GetUserByUuid(minecraftUuid);
        uow.CompleteAndCommit();
        return updatedUser is null ? null : User.FromDbModel(updatedUser);
    }
}