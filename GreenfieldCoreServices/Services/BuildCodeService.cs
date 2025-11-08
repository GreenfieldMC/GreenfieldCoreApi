using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.BuildCodes;
using GreenfieldCoreServices.Services.Interfaces;

namespace GreenfieldCoreServices.Services;

public class BuildCodeService(IUnitOfWork uow, ICacheService<long, BuildCode> cache) : IBuildCodeService
{
    public async Task<IEnumerable<BuildCode>> GetAllBuildCodes(bool showDeleted = false)
    {
        if (cache.GetCount() > 0) return cache.GetValues().Where(bc => showDeleted || !bc.Deleted);
        
        var repo = uow.Repository<IBuildCodeRepository>();
        var buildCodes = (await repo.GetAllBuildCodes()).GetNonNullOrThrow();
        
        var result = buildCodes.Select(BuildCode.FromDbModel).ToList();
        
        foreach (var buildCode in result) 
            cache.SetValue(buildCode.BuildCodeId, buildCode);
        
        return result.Where(bc => showDeleted || !bc.Deleted);
    }

    public async Task<BuildCode?> GetBuildCodeById(long buildCodeId)
    {
        if (cache.TryGetValue(buildCodeId, out var cachedBuildCode))
            return cachedBuildCode;
        
        var repo = uow.Repository<IBuildCodeRepository>();
        var buildCodeEntity = (await repo.GetBuildCode(buildCodeId)).GetOrThrow();
        if (buildCodeEntity is null) return null;
        
        var buildCode = BuildCode.FromDbModel(buildCodeEntity);
        cache.SetValue(buildCode.BuildCodeId, buildCode);
        return buildCode;
    }

    public async Task<BuildCode?> DeleteBuildCode(long buildCodeId)
    {
        var foundBuildCode = await GetBuildCodeById(buildCodeId);
        if (foundBuildCode is null) return null;
        
        var repo = uow.Repository<IBuildCodeRepository>();
        
        uow.BeginTransaction();
        var deleteResult = (await repo.DeleteBuildCode(buildCodeId)).GetOrThrow();
        
        if (!deleteResult) return null;
        uow.CompleteAndCommit();
        
        foundBuildCode.Deleted = true;
        cache.SetValue(foundBuildCode.BuildCodeId, foundBuildCode);
        return foundBuildCode;
    }

    public async Task<BuildCode?> CreateBuildCode(int listOrder, string buildCode)
    {
        var repo = uow.Repository<IBuildCodeRepository>();
        
        uow.BeginTransaction();
        var created = (await repo.CreateBuildCode(listOrder, buildCode)).GetNonNullOrThrow(nullDataMessage: "The build code could not be created.");
        uow.CompleteAndCommit();
        
        var buildCodeModel = BuildCode.FromDbModel(created);
        cache.SetValue(buildCodeModel.BuildCodeId, buildCodeModel);
        return buildCodeModel;
    }

    public async Task<BuildCode?> UpdateBuildCode(long buildCodeId, int? listOrder = null, string? buildCode = null)
    {
        var foundBuildCode = await GetBuildCodeById(buildCodeId);
        if (foundBuildCode is null) return null;
        
        var repo = uow.Repository<IBuildCodeRepository>();
        
        uow.BeginTransaction();
        var updateResult = (await repo.UpdateBuildCode(
            buildCodeId,
            listOrder ?? foundBuildCode.ListOrder,
            buildCode ?? foundBuildCode.Code)).GetOrThrow();
        
        if (!updateResult) return null;
        uow.CompleteAndCommit();
        
        foundBuildCode.ListOrder = listOrder ?? foundBuildCode.ListOrder;
        foundBuildCode.Code = buildCode ?? foundBuildCode.Code;
        cache.SetValue(foundBuildCode.BuildCodeId, foundBuildCode);
        return foundBuildCode;
    }
}