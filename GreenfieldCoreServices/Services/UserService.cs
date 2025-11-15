using System.Net;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.Interfaces;

namespace GreenfieldCoreServices.Services;

public class UserService(IUnitOfWork uow) : IUserService
{
    public async Task<Result<User>> CreateUser(Guid minecraftUuid, string username)
    {
        if (!IsValidUsername(username))
            return Result<User>.Failure("A valid username must be provided.");
        var repo = uow.Repository<IUserRepository>();
        var foundUser = (await repo.GetUserByUuid(minecraftUuid)).GetOrThrow();
        if (foundUser is not null) return Result<User>.Failure("User already exists.", HttpStatusCode.Conflict);
        uow.BeginTransaction();
        var created = (await repo.CreateUser(minecraftUuid, username)).GetOrThrow();
        if (created is null) return Result<User>.Failure("User could not be created.");
        uow.CompleteAndCommit();
        return Result<User>.Success(User.FromDbModel(created));
    }

    public async Task<Result<User>> GetUserByUuid(Guid minecraftUuid)
    {
        var repo = uow.Repository<IUserRepository>();
        var foundUser = (await repo.GetUserByUuid(minecraftUuid)).GetOrThrow();
        return foundUser is null ? Result<User>.Failure("User not found.", HttpStatusCode.NotFound) : Result<User>.Success(User.FromDbModel(foundUser));
    }

    public async Task<Result<User>> GetUserByUserId(long userId)
    {
        var repo = uow.Repository<IUserRepository>();
        var foundUser = (await repo.GetUserByUserId(userId)).GetOrThrow();
        return foundUser is null ? Result<User>.Failure("User not found.", HttpStatusCode.NotFound) : Result<User>.Success(User.FromDbModel(foundUser));
    }

    public async Task<Result<User>> UpdateUsername(Guid minecraftUuid, string newUsername)
    {
        // Guid.Empty is the System user, can skip validation.
        if (!IsValidUsername(newUsername) && minecraftUuid != Guid.Empty)
            return Result<User>.Failure("A valid new username must be provided.");
        
        var repo = uow.Repository<IUserRepository>();
        
        var existingUserResult = await GetUserByUuid(minecraftUuid);
        if (!existingUserResult.IsSuccessful) return existingUserResult;
        var existingUser = existingUserResult.GetNonNullOrThrow();
        
        uow.BeginTransaction();
        var updateResult = (await repo.UpdateUsername(minecraftUuid, newUsername)).GetOrThrow();
        if (!updateResult) return Result<User>.Failure("Username could not be updated.");
        uow.CompleteAndCommit();
        existingUser.Username = newUsername;
        return Result<User>.Success(existingUser);
    }
    
    private bool IsValidUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;
        return username.Length is >= 3 and <= 16 && username.All(char.IsLetterOrDigit);
    }
}