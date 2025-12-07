using System.Net;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.Interfaces;

namespace GreenfieldCoreServices.Services;

public class UserService(IUnitOfWork uow, ICacheService<long, User> userCache, ICacheService<long, List<ulong>> discordCache) : IUserService
{
    public async Task<Result<User>> CreateUser(Guid minecraftUuid, string username)
    {
        if (!IsValidUsername(username))
            return Result<User>.Failure("A valid username must be provided.");
        var repo = uow.Repository<IUserRepository>();
        
        var didFindUser = userCache.TryGetValue(u => u.MinecraftUuid == minecraftUuid, out _) || (await repo.GetUserByUuid(minecraftUuid)).GetOrThrow() is not null;
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
        var foundUser = (await repo.GetUserByUuid(minecraftUuid)).GetOrThrow();
        return foundUser is null ? Result<User>.Failure("User not found.", HttpStatusCode.NotFound) : Result<User>.Success(User.FromDbModel(foundUser));
    }

    public async Task<Result<User>> GetUserByUserId(long userId)
    {
        if (userCache.TryGetValue(userId, out var cachedUser))
            return Result<User>.Success(cachedUser);
        
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
        userCache.SetValue(existingUser.UserId, existingUser);
        return Result<User>.Success(existingUser);
    }

    public async Task<Result<bool>> LinkDiscordAccount(long userId, ulong discordSnowflake)
    {
        var existingUserResult = await GetUserByUserId(userId);
        if (!existingUserResult.IsSuccessful) return Result<bool>.Failure("User not found.", HttpStatusCode.NotFound);

        var linkedAccountsResult = await GetLinkedDiscordAccounts(userId);
        if (!linkedAccountsResult.IsSuccessful)
            return Result<bool>.Failure("Could not retrieve linked Discord accounts.", linkedAccountsResult.StatusCode);
        
        var linkedAccounts = linkedAccountsResult.GetNonNullOrThrow().ToList();
        if (linkedAccounts.Contains(discordSnowflake))
            return Result<bool>.Failure("Discord account is already linked to this user.", HttpStatusCode.Conflict);
        
        var repo = uow.Repository<IUserRepository>();
        
        uow.BeginTransaction();
        var createdLink = (await repo.CreateUserDiscordReference(userId, discordSnowflake)).GetOrThrow();
        uow.CompleteAndCommit();
        
        discordCache.SetValue(userId, linkedAccounts.Append(discordSnowflake).ToList());
        return createdLink is null ? Result<bool>.Failure("Discord account could not be linked.") : Result<bool>.Success(true);
    }

    public async Task<Result<bool>> UnlinkDiscordAccount(long userId, ulong discordSnowflake)
    {
        var existingUserResult = await GetUserByUserId(userId);
        if (!existingUserResult.IsSuccessful) return Result<bool>.Failure("User not found.", HttpStatusCode.NotFound);
        
        var linkedAccountsResult = await GetLinkedDiscordAccounts(userId);
        if (!linkedAccountsResult.IsSuccessful)
            return Result<bool>.Failure("Could not retrieve linked Discord accounts.", linkedAccountsResult.StatusCode);
        
        var linkedAccounts = linkedAccountsResult.GetNonNullOrThrow().ToList();
        if (!linkedAccounts.Contains(discordSnowflake))
            return Result<bool>.Failure("Discord account is not linked to this user.", HttpStatusCode.NotFound);
        
        var repo = uow.Repository<IUserRepository>();
        uow.BeginTransaction();
        var deleteResult = (await repo.DeleteUserDiscordReference(userId, discordSnowflake)).GetOrThrow();
        if (!deleteResult) return Result<bool>.Failure("Discord account could not be unlinked.");
        uow.CompleteAndCommit();
        discordCache.SetValue(userId, linkedAccounts.Where(id => id != discordSnowflake).ToList());
        return Result<bool>.Success(true);
    }

    public async Task<Result<IEnumerable<ulong>>> GetLinkedDiscordAccounts(long userId)
    {
        var userResult = await GetUserByUserId(userId);
        if (!userResult.IsSuccessful) return Result<IEnumerable<ulong>>.Failure("User not found.", userResult.StatusCode);
        var user = userResult.GetNonNullOrThrow();
        return await GetLinkedDiscordAccountsInternal(user.UserId);
    }

    public async Task<Result<IEnumerable<ulong>>> GetLinkedDiscordAccountsByUuid(Guid minecraftUuid)
    {
        var userResult = await GetUserByUuid(minecraftUuid);
        if (!userResult.IsSuccessful) return Result<IEnumerable<ulong>>.Failure("User not found.", userResult.StatusCode);
        var user = userResult.GetNonNullOrThrow();
        return await GetLinkedDiscordAccountsInternal(user.UserId);
    }
    
    private async Task<Result<IEnumerable<ulong>>> GetLinkedDiscordAccountsInternal(long userId)
    {
        if (discordCache.TryGetValue(userId, out var cachedDiscordIds))
            return Result<IEnumerable<ulong>>.Success(cachedDiscordIds);
        
        var repo = uow.Repository<IUserRepository>();
        var linkedEntities = (await repo.GetUserDiscordReferences(userId)).GetNonNullOrThrow();
        var discordIds = linkedEntities.Select(e => e.DiscordSnowflake).ToList();
        discordCache.SetValue(userId, discordIds);
        return Result<IEnumerable<ulong>>.Success(discordIds);
    }
    
    private bool IsValidUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;
        return username.Length is >= 3 and <= 16 && username.All(char.IsLetterOrDigit);
    }
}