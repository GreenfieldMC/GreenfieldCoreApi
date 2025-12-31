using System.Net;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.Interfaces;

namespace GreenfieldCoreServices.Services;

public class PatreonService(IUnitOfWork uow, ICacheService<long, UserPatreonAccount> patreonCache) : IPatreonService
{
    
    public async Task<Result<IEnumerable<UserPatreonAccount>>> GetAllPatreonAccounts()
    {
        var repo = uow.Repository<IUserPatreonRepository>();
        var selectAllResult = await repo.SelectAllPatreonAccounts();
        
        return selectAllResult.TryGetDataNonNull(out var patreonAccounts)
            ? Result<IEnumerable<UserPatreonAccount>>.Success(patreonAccounts.Select(UserPatreonAccount.FromDbModel))
            : Result<IEnumerable<UserPatreonAccount>>.Failure("Failed to retrieve Patreon accounts.", selectAllResult.StatusCode);
    }

    public async Task<Result<UserPatreonAccount>> CreatePatreonAccountReference(long userId, long patreonId, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope, string fullName, decimal? pledge)
    {
        var repo = uow.Repository<IUserPatreonRepository>();
        
        uow.BeginTransaction();
        var createReferenceResult = await repo.InsertUserPatreonReference(userId, patreonId, refreshToken, accessToken, tokenType, tokenExpiry, scope, fullName, pledge);
        if (!createReferenceResult.TryGetDataNonNull(out var patreonEntity))
            return Result<UserPatreonAccount>.Failure($"Failed to link Patreon account. {createReferenceResult.ErrorMessage}");
        uow.CompleteAndCommit();
        
        var account = UserPatreonAccount.FromDbModel(patreonEntity);
        patreonCache.SetValue(account.UserPatreonId, account);

        return Result<UserPatreonAccount>.Success(account);
    }

    public async Task<Result<UserPatreonAccount>> UpdatePatreonAccountTokens(long userId, long patreonId, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope)
    {
        var repo = uow.Repository<IUserPatreonRepository>();
        
        uow.BeginTransaction();
        var result = await repo.UpdateUserPatreonTokens(userId, patreonId, refreshToken, accessToken, tokenType, tokenExpiry, scope);
        if (!result.TryGetDataNonNull(out var updatedTokens) || !updatedTokens)
            return Result<UserPatreonAccount>.Failure("Failed to update Patreon account tokens.");
        uow.CompleteAndCommit();
        
        patreonCache.RemoveValues(a => a.UserId == userId && a.PatreonId == patreonId);
        
        var patreonAccountResult = await GetPatreonAccountByUserIdAndPatreonId(userId, patreonId);
        return !patreonAccountResult.IsSuccessful 
            ? Result<UserPatreonAccount>.Failure($"Failed to retrieve updated Patreon account. {patreonAccountResult.ErrorMessage}", patreonAccountResult.StatusCode)
            : patreonAccountResult;
    }

    public async Task<Result<UserPatreonAccount>> UpdatePatreonAccountInfo(long userId, long patreonId, string fullName, decimal? pledge)
    {
        var repo = uow.Repository<IUserPatreonRepository>();
        
        uow.BeginTransaction();
        var result = await repo.UpdateUserPatreonInfo(userId, patreonId, fullName, pledge);
        if (!result.TryGetDataNonNull(out var updatedUser))
            return Result<UserPatreonAccount>.Failure("Failed to update Patreon pledge amount.");
        uow.CompleteAndCommit();
        
        patreonCache.RemoveValues(a => a.UserId == userId && a.PatreonId == patreonId);
        patreonCache.SetValue(updatedUser.UserPatreonId, UserPatreonAccount.FromDbModel(updatedUser));

        return Result<UserPatreonAccount>.Success(UserPatreonAccount.FromDbModel(updatedUser));
    }

    public async Task<Result> UnlinkPatreonAccountReference(long userId, long patreonId)
    {
        var repo = uow.Repository<IUserPatreonRepository>();
        uow.BeginTransaction();
        var deleteResult = await repo.DeleteUserPatreonReference(userId, patreonId);
        if (!deleteResult.TryGetDataNonNull(out var result) || !result) return Result.Failure("Patreon account could not be unlinked.");
        uow.CompleteAndCommit();

        patreonCache.RemoveValues(a => a.PatreonId == patreonId && a.UserId == userId);

        return Result.Success();
    }

    public async Task<Result<UserPatreonAccount>> GetPatreonAccountByUserIdAndPatreonId(long userId, long patreonId)
    {
        if (patreonCache.TryGetValue(a => a.UserId == userId && a.PatreonId == patreonId, out var cachedAccount))
            return Result<UserPatreonAccount>.Success(cachedAccount);
        
        var repo = uow.Repository<IUserPatreonRepository>();
        
        var accountResult = await repo.SelectUserPatreonAccount(userId, patreonId);
        if (!accountResult.TryGetDataNonNull(out var patreonAccount))
            return Result<UserPatreonAccount>.Failure("Patreon account not found.", HttpStatusCode.NotFound);
        
        var mappedAccount = UserPatreonAccount.FromDbModel(patreonAccount);
        patreonCache.SetValue(mappedAccount.UserPatreonId, mappedAccount);
        return Result<UserPatreonAccount>.Success(mappedAccount);
    }

    public async Task<Result<IEnumerable<UserPatreonAccount>>> GetPatreonAccountsByUserId(long userId)
    {
        if (patreonCache.TryGetValues(a => a.UserId == userId, out var cachedAccounts))
            return Result<IEnumerable<UserPatreonAccount>>.Success(cachedAccounts);
        
        var repo = uow.Repository<IUserPatreonRepository>();
        var patreonAccountsResult = await repo.SelectUserPatreonReferences(userId);
        if (!patreonAccountsResult.IsSuccessful) return Result<IEnumerable<UserPatreonAccount>>.Failure("Failed to retrieve Patreon accounts.", patreonAccountsResult.StatusCode);

        var patreonAccounts = patreonAccountsResult.GetOrDefault([]);
        var mappedAccounts = patreonAccounts.Select(UserPatreonAccount.FromDbModel).ToList();
        
        foreach (var account in mappedAccounts)
            patreonCache.SetValue(account.UserPatreonId, account);
        
        return Result<IEnumerable<UserPatreonAccount>>.Success(mappedAccounts);
    }

    public async Task<Result<IEnumerable<UserPatreonAccount>>> GetPatreonAccountsByPatreonId(long patreonId)
    {
        if (patreonCache.TryGetValues(a => a.PatreonId == patreonId, out var cachedAccounts))
            return Result<IEnumerable<UserPatreonAccount>>.Success(cachedAccounts);
        
        var repo = uow.Repository<IUserPatreonRepository>();
        var patreonAccountsResult = await repo.SelectUserPatreonAccountByPatreonId(patreonId);
        if (!patreonAccountsResult.IsSuccessful) return Result<IEnumerable<UserPatreonAccount>>.Failure("Failed to retrieve Patreon accounts.", patreonAccountsResult.StatusCode);

        var patreonAccounts = patreonAccountsResult.GetOrDefault([]);
        var mappedAccounts = patreonAccounts.Select(UserPatreonAccount.FromDbModel).ToList();

        foreach (var account in mappedAccounts)
            patreonCache.SetValue(account.UserPatreonId, account);

        return Result<IEnumerable<UserPatreonAccount>>.Success(mappedAccounts);
    }
}