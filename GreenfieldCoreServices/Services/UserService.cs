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
        var foundUser = (await repo.GetUserByUuid(minecraftUuid)).GetOrThrow();
        if (foundUser is not null) return null;
        uow.BeginTransaction();
        var created = (await repo.CreateUser(minecraftUuid, username)).GetOrThrow();
        if (created is null) return null;
        uow.CompleteAndCommit();
        return User.FromDbModel(created);
    }

    public async Task<User?> GetUserByUuid(Guid minecraftUuid)
    {
        var repo = uow.Repository<IUserRepository>();
        var foundUser = (await repo.GetUserByUuid(minecraftUuid)).GetOrThrow();
        return foundUser is null ? null : User.FromDbModel(foundUser);
    }

    public async Task<User?> GetUserByUserId(long userId)
    {
        var repo = uow.Repository<IUserRepository>();
        var foundUser = (await repo.GetUserByUserId(userId)).GetOrThrow();
        return foundUser is null ? null : User.FromDbModel(foundUser);
    }

    public async Task<User?> UpdateUsername(Guid minecraftUuid, string newUsername)
    {
        var repo = uow.Repository<IUserRepository>();
        uow.BeginTransaction();
        var updateResult = (await repo.UpdateUsername(minecraftUuid, newUsername)).GetOrThrow();
        if (!updateResult) return null;
        var updatedUser = (await repo.GetUserByUuid(minecraftUuid)).GetOrThrow();
        uow.CompleteAndCommit();
        return updatedUser is null ? null : User.FromDbModel(updatedUser);
    }
}