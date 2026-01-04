using System.Net;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.Interfaces;

namespace GreenfieldCoreServices.Services;

public class UserService(IUnitOfWork uow, ICacheService<long, User> userCache) : IUserService
{
    public async Task<Result<User>> CreateUser(Guid minecraftUuid, string username)
    {
        if (!IsValidUsername(username))
            return Result<User>.Failure("A valid username must be provided.");
        var repo = uow.Repository<IUserRepository>();
        
        var didFindUser = userCache.TryGetValue(u => u.MinecraftUuid == minecraftUuid, out _) || (await repo.SelectUserByUuid(minecraftUuid)).GetOrThrow() is not null;
        if (didFindUser) return Result<User>.Failure("User already exists.", HttpStatusCode.Conflict);
        
        uow.BeginTransaction();
        var created = (await repo.CreateUser(minecraftUuid, username)).GetOrThrow();
        if (created is null) return Result<User>.Failure("User could not be created.");
        uow.CompleteAndCommit();
        var createdUser = User.FromDbModel(created);
        userCache.SetValue(createdUser.UserId, createdUser);
        return Result<User>.Success(createdUser);
    }

    public async Task<Result<User>> GetUserByUuid(Guid minecraftUuid)
    {
        if (userCache.TryGetValue(u => u.MinecraftUuid == minecraftUuid, out var cachedUser))
            return Result<User>.Success(cachedUser);
        
        var repo = uow.Repository<IUserRepository>();
        var foundUser = (await repo.SelectUserByUuid(minecraftUuid)).GetOrThrow();
        return foundUser is null ? Result<User>.Failure("User not found.", HttpStatusCode.NotFound) : Result<User>.Success(User.FromDbModel(foundUser));
    }

    public async Task<Result<User>> GetUserByUserId(long userId)
    {
        if (userCache.TryGetValue(userId, out var cachedUser))
            return Result<User>.Success(cachedUser);
        
        var repo = uow.Repository<IUserRepository>();
        var foundUser = (await repo.SelectUserByUserId(userId)).GetOrThrow();
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
        var updateResult = await repo.UpdateUsername(minecraftUuid, newUsername);
        if (!updateResult.IsSuccessful) return Result<User>.Failure(updateResult.ErrorMessage ?? "Failed to update username.");
        uow.CompleteAndCommit();
        
        existingUser.Username = newUsername;
        userCache.SetValue(existingUser.UserId, existingUser);
        return Result<User>.Success(existingUser);
    }
    
    private bool IsValidUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;
        return username.Length is >= 3 and <= 16 && username.All(char.IsLetterOrDigit);
    }
}