using System.Net;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.BuildCodes;
using GreenfieldCoreServices.Services.Interfaces;

namespace GreenfieldCoreServices.Services;

public class BuildCodeService(IUnitOfWork uow, ICacheService<long, BuildCode> cache) : IBuildCodeService
{
    public async Task<Result<IEnumerable<BuildCode>>> GetAllBuildCodes(bool showDeleted = false)
    {
        if (cache.GetCount() > 0) return Result<IEnumerable<BuildCode>>.Success(cache.GetValues().Where(bc => showDeleted || !bc.Deleted));
        
        var repo = uow.Repository<IBuildCodeRepository>();
        var buildCodes = (await repo.GetAllBuildCodes()).GetNonNullOrThrow();
        
        var result = buildCodes.Select(BuildCode.FromDbModel).ToList();
        
        foreach (var buildCode in result) 
            cache.SetValue(buildCode.BuildCodeId, buildCode);
        
        return Result<IEnumerable<BuildCode>>.Success(result.Where(bc => showDeleted || !bc.Deleted));
    }

    public async Task<Result<BuildCode>> GetBuildCodeById(long buildCodeId)
    {
        if (cache.TryGetValue(buildCodeId, out var cachedBuildCode))
            return Result<BuildCode>.Success(cachedBuildCode);
        
        var repo = uow.Repository<IBuildCodeRepository>();
        var buildCodeEntity = (await repo.GetBuildCode(buildCodeId)).GetOrThrow();
        if (buildCodeEntity is null) return Result<BuildCode>.Failure($"Build code id {buildCodeId} not found.", HttpStatusCode.NotFound);
        
        var buildCode = BuildCode.FromDbModel(buildCodeEntity);
        cache.SetValue(buildCode.BuildCodeId, buildCode);
        return Result<BuildCode>.Success(buildCode);
    }

    public async Task<Result<BuildCode>> DeleteBuildCode(long buildCodeId)
    {
        var foundBuildCodeResult = await GetBuildCodeById(buildCodeId);
        if (!foundBuildCodeResult.IsSuccessful) return foundBuildCodeResult;
        var foundBuildCode = foundBuildCodeResult.GetNonNullOrThrow(nullDataMessage: "GetBuildCodeById returned null unexpectedly despite being successful.");
        
        var repo = uow.Repository<IBuildCodeRepository>();
        
        uow.BeginTransaction();
        var deleteResult = (await repo.DeleteBuildCode(buildCodeId)).GetOrThrow();
        
        if (!deleteResult) return Result<BuildCode>.Failure($"Build code id {buildCodeId} could not be deleted.");
        uow.CompleteAndCommit();
        
        foundBuildCode.Deleted = true;
        cache.SetValue(foundBuildCode.BuildCodeId, foundBuildCode);
        return Result<BuildCode>.Success(foundBuildCode);
    }

    public async Task<Result<BuildCode>> CreateBuildCode(int listOrder, string buildCode)
    {
        if (string.IsNullOrWhiteSpace(buildCode))
            return Result<BuildCode>.Failure("A valid build code must be provided.");
        
        var repo = uow.Repository<IBuildCodeRepository>();
        
        uow.BeginTransaction();
        //there isn't really a reason the BuildCode couldn't be created aside from a DB issue
        var created = (await repo.CreateBuildCode(listOrder, buildCode)).GetNonNullOrThrow(nullDataMessage: "The build code could not be created.");
        uow.CompleteAndCommit();
        
        var buildCodeModel = BuildCode.FromDbModel(created);
        cache.SetValue(buildCodeModel.BuildCodeId, buildCodeModel);
        return Result<BuildCode>.Success(buildCodeModel);
    }

    public async Task<Result<BuildCode>> UpdateBuildCode(long buildCodeId, int? listOrder = null,
        string? buildCode = null)
    {
        if (listOrder is null && buildCode is null)
            return Result<BuildCode>.Failure("At least one of listOrder or buildCode must be provided for update.");
        
        var foundBuildCodeResult = await GetBuildCodeById(buildCodeId);
        if (!foundBuildCodeResult.IsSuccessful) return foundBuildCodeResult;
        var foundBuildCode = foundBuildCodeResult.GetNonNullOrThrow(nullDataMessage: "GetBuildCodeById returned null unexpectedly despite being successful.");
        
        var repo = uow.Repository<IBuildCodeRepository>();
        
        if (buildCode is not null && string.IsNullOrWhiteSpace(buildCode))
            return Result<BuildCode>.Failure("A valid build code must be provided for update.");
        
        uow.BeginTransaction();
        var updateResult = (await repo.UpdateBuildCode(
            buildCodeId,
            listOrder ?? foundBuildCode.ListOrder,
            buildCode ?? foundBuildCode.Code)).GetOrThrow();
        
        if (!updateResult) return Result<BuildCode>.Failure($"Build code id {buildCodeId} could not be updated.");
        uow.CompleteAndCommit();
        
        foundBuildCode.ListOrder = listOrder ?? foundBuildCode.ListOrder;
        foundBuildCode.Code = buildCode ?? foundBuildCode.Code;
        cache.SetValue(foundBuildCode.BuildCodeId, foundBuildCode);
        return Result<BuildCode>.Success(foundBuildCode);
    }
}