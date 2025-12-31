using System.Collections.Generic;
using System.Linq;
using System.Net;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.Interfaces;

namespace GreenfieldCoreServices.Services;

public class DiscordService(IUnitOfWork uow, ICacheService<long, UserDiscordAccount> discordAccountCache) : IDiscordService
{
    public async Task<Result<IEnumerable<UserDiscordAccount>>> GetAllDiscordAccounts()
    {
        var repo = uow.Repository<IUserDiscordRepository>();
        var selectAllResult = await repo.SelectAllDiscordAccounts();
        return selectAllResult.TryGetDataNonNull(out var accounts)
            ? Result<IEnumerable<UserDiscordAccount>>.Success(accounts.Select(UserDiscordAccount.FromDbModel))
            : Result<IEnumerable<UserDiscordAccount>>.Failure("Failed to retrieve Discord accounts.", selectAllResult.StatusCode);
    }

    public async Task<Result<IEnumerable<UserDiscordAccount>>> GetDiscordAccountsBySnowflake(ulong discordSnowflake)
    {
        if (discordAccountCache.TryGetValues(a => a.DiscordSnowflake == discordSnowflake, out var cached))
            return Result<IEnumerable<UserDiscordAccount>>.Success(cached);
        
        var repo = uow.Repository<IUserDiscordRepository>();
        var selectResult = await repo.SelectDiscordAccountsBySnowflake(discordSnowflake);
        if (!selectResult.IsSuccessful) 
            return Result<IEnumerable<UserDiscordAccount>>.Failure("Failed to retrieve Discord accounts.", selectResult.StatusCode);
        
        var discordAccounts = selectResult.GetOrDefault([]);
        var mappedAccounts = discordAccounts.Select(UserDiscordAccount.FromDbModel).ToList();
        
        foreach (var account in mappedAccounts)
            discordAccountCache.SetValue(account.UserDiscordId, account);
        
        return Result<IEnumerable<UserDiscordAccount>>.Success(mappedAccounts);
    }

    public async Task<Result<UserDiscordAccount>> CreateDiscordAccountReference(long userId, ulong discordSnowflake, string? discordUsername, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope)
    {
        var repo = uow.Repository<IUserDiscordRepository>();
        uow.BeginTransaction();
        var createResult = await repo.InsertUserDiscordReference(userId, discordSnowflake, discordUsername, refreshToken, accessToken, tokenType, tokenExpiry, scope);
        if (!createResult.TryGetDataNonNull(out var entity))
            return Result<UserDiscordAccount>.Failure($"Failed to link Discord account. {createResult.ErrorMessage}");
        uow.CompleteAndCommit();

        var account = UserDiscordAccount.FromDbModel(entity);
        discordAccountCache.SetValue(account.UserDiscordId, account);
        return Result<UserDiscordAccount>.Success(account);
    }

    public async Task<Result<UserDiscordAccount>> UpdateDiscordAccountTokens(long userId, ulong discordSnowflake, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope)
    {
        var repo = uow.Repository<IUserDiscordRepository>();
        uow.BeginTransaction();
        var updateResult = await repo.UpdateUserDiscordTokens(userId, discordSnowflake, refreshToken, accessToken, tokenType, tokenExpiry, scope);
        if (!updateResult.TryGetDataNonNull(out var updated) || !updated)
            return Result<UserDiscordAccount>.Failure("Failed to update Discord account tokens.");
        uow.CompleteAndCommit();

        discordAccountCache.RemoveValues(a => a.UserId == userId && a.DiscordSnowflake == discordSnowflake);
        var reloadResult = await GetDiscordAccountByUserIdAndSnowflake(userId, discordSnowflake);
        return reloadResult.IsSuccessful
            ? reloadResult
            : Result<UserDiscordAccount>.Failure(reloadResult.ErrorMessage ?? "Failed to reload Discord account.", reloadResult.StatusCode);
    }

    public async Task<Result<UserDiscordAccount>> UpdateDiscordAccountProfile(long userId, ulong discordSnowflake, string? discordUsername)
    {
        var repo = uow.Repository<IUserDiscordRepository>();
        uow.BeginTransaction();
        var updateResult = await repo.UpdateUserDiscordProfile(userId, discordSnowflake, discordUsername ?? string.Empty);
        if (!updateResult.TryGetDataNonNull(out var updated) || !updated)
            return Result<UserDiscordAccount>.Failure("Failed to update Discord account profile.");
        uow.CompleteAndCommit();

        discordAccountCache.RemoveValues(a => a.UserId == userId && a.DiscordSnowflake == discordSnowflake);
        var reloadResult = await GetDiscordAccountByUserIdAndSnowflake(userId, discordSnowflake);
        return reloadResult.IsSuccessful
            ? reloadResult
            : Result<UserDiscordAccount>.Failure(reloadResult.ErrorMessage ?? "Failed to reload Discord account.", reloadResult.StatusCode);
    }

    public async Task<Result> UnlinkDiscordAccountReference(long userId, ulong discordSnowflake)
    {
        var repo = uow.Repository<IUserDiscordRepository>();
        uow.BeginTransaction();
        var deleteResult = await repo.DeleteUserDiscordReference(userId, discordSnowflake);
        if (!deleteResult.TryGetDataNonNull(out var deleted) || !deleted)
            return Result.Failure("Discord account could not be unlinked.");
        uow.CompleteAndCommit();

        discordAccountCache.RemoveValues(a => a.UserId == userId && a.DiscordSnowflake == discordSnowflake);
        return Result.Success();
    }

    public async Task<Result<UserDiscordAccount>> GetDiscordAccountByUserIdAndSnowflake(long userId, ulong discordSnowflake)
    {
        if (discordAccountCache.TryGetValue(a => a.UserId == userId && a.DiscordSnowflake == discordSnowflake, out var cached))
            return Result<UserDiscordAccount>.Success(cached);

        var repo = uow.Repository<IUserDiscordRepository>();
        var accountResult = await repo.SelectUserDiscordAccount(userId, discordSnowflake);
        if (!accountResult.TryGetDataNonNull(out var entity))
            return Result<UserDiscordAccount>.Failure("Discord account not found.", HttpStatusCode.NotFound);

        var mapped = UserDiscordAccount.FromDbModel(entity);
        discordAccountCache.SetValue(mapped.UserDiscordId, mapped);
        return Result<UserDiscordAccount>.Success(mapped);
    }

    public async Task<Result<IEnumerable<UserDiscordAccount>>> GetDiscordAccountsByUserId(long userId)
    {
        if (discordAccountCache.TryGetValues(a => a.UserId == userId, out var cached))
            return Result<IEnumerable<UserDiscordAccount>>.Success(cached);

        var repo = uow.Repository<IUserDiscordRepository>();
        var accountsResult = await repo.SelectUserDiscordReferences(userId);
        if (!accountsResult.TryGetDataNonNull(out var entities))
            return Result<IEnumerable<UserDiscordAccount>>.Failure("Failed to retrieve Discord accounts.", accountsResult.StatusCode);

        var mapped = entities.Select(UserDiscordAccount.FromDbModel).ToList();
        foreach (var account in mapped)
            discordAccountCache.SetValue(account.UserDiscordId, account);
        return Result<IEnumerable<UserDiscordAccount>>.Success(mapped);
    }
}
