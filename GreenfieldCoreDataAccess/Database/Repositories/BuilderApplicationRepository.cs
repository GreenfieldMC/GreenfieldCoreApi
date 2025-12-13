using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Net;
using Dapper;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class BuilderApplicationRepository(IUnitOfWork unitOfWork) : BaseRepository(unitOfWork), IBuilderApplicationRepository
{
    private const string InsertBuilderAppProc = "usp_InsertBuilderApplication";
    private const string InsertBuilderAppStatusProc = "usp_InsertBuilderApplicationStatus";
    private const string InsertBuilderAppImageProc = "usp_InsertBuilderApplicationImage";
    private const string SelectBuilderAppsByUserProc = "usp_SelectBuilderApplicationsByUser";
    private const string SelectBuilderAppImagesProc = "usp_SelectBuilderApplicationImages";
    private const string SelectBuilderAppStatusesProc = "usp_SelectBuilderApplicationStatuses";
    private const string SelectBuilderAppByIdProc = "usp_SelectBuilderApplicationById";

    public async Task<Result<IEnumerable<BuilderApplicationEntity>>> GetApplicationsByUser(long userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        try
        {
            var entities = await Connection.QueryAsync<BuilderApplicationEntity>(SelectBuilderAppsByUserProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<IEnumerable<BuilderApplicationEntity>>.Success(entities);
        }
        catch (DbException ex)
        {
            return Result<IEnumerable<BuilderApplicationEntity>>.Failure($"Failed to retrieve builder applications: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<BuilderAppImageLinkEntity>>> GetApplicationImages(long applicationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ApplicationId", applicationId, DbType.Int64);
        try
        {
            var images = await Connection.QueryAsync<BuilderAppImageLinkEntity>(SelectBuilderAppImagesProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<IEnumerable<BuilderAppImageLinkEntity>>.Success(images);
        }
        catch (DbException ex)
        {
            return Result<IEnumerable<BuilderAppImageLinkEntity>>.Failure($"Failed to retrieve builder application images: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<BuilderAppStatusEntity>>> GetStatusesByApplication(long applicationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ApplicationId", applicationId, DbType.Int64);
        try
        {
            var statuses = await Connection.QueryAsync<BuilderAppStatusEntity>(SelectBuilderAppStatusesProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<IEnumerable<BuilderAppStatusEntity>>.Success(statuses);
        }
        catch (DbException ex)
        {
            return Result<IEnumerable<BuilderAppStatusEntity>>.Failure($"Failed to retrieve builder application statuses: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<BuilderApplicationEntity>> InsertApplication(
        long userId,
        int userAge,
        string? userNationality,
        string? additionalBuildingInformation,
        string whyJoinGreenfield,
        string? additionalComments)
    {
        var insertParameters = new DynamicParameters();
        insertParameters.Add("p_UserId", userId, DbType.Int64);
        insertParameters.Add("p_UserAge", userAge, DbType.Int32);
        insertParameters.Add("p_UserNationality", userNationality, DbType.String, size: 128);
        insertParameters.Add("p_AdditionalBuildingInformation", additionalBuildingInformation, DbType.String, size: 4096);
        insertParameters.Add("p_WhyJoinGreenfield", whyJoinGreenfield, DbType.String, size: 4096);
        insertParameters.Add("p_AdditionalComments", additionalComments, DbType.String, size: 4096);

        try
        {
            var application = await Connection.QuerySingleAsync<BuilderApplicationEntity>(InsertBuilderAppProc, insertParameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<BuilderApplicationEntity>.Success(application);
        }
        catch (DbException ex)
        {
            return Result<BuilderApplicationEntity>.Failure($"Failed to insert builder application: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<bool>> InsertStatus(long applicationId, string status, string? statusMessage)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ApplicationId", applicationId, DbType.Int64);
        parameters.Add("p_Status", status, DbType.String, size: 256);
        parameters.Add("p_StatusMessage", statusMessage, DbType.String, size: 4096);
        try
        {
            var rows = await Connection.ExecuteAsync(InsertBuilderAppStatusProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<bool>.Success(rows > 0);
        }
        catch (DbException ex)
        {
            return Result<bool>.Failure($"Failed to insert builder application status: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<bool>> InsertImage(long applicationId, string linkType, string imageLink)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ApplicationId", applicationId, DbType.Int64);
        parameters.Add("p_LinkType", linkType, DbType.String, size: 256);
        parameters.Add("p_ImageLink", imageLink, DbType.String, size: 2048);
        try
        {
            var rows = await Connection.ExecuteAsync(InsertBuilderAppImageProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<bool>.Success(rows > 0);
        }
        catch (DbException ex)
        {
            return Result<bool>.Failure(ex.Message, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<BuilderApplicationEntity>> GetApplicationById(long applicationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ApplicationId", applicationId, DbType.Int64);
        try
        {
            var application = await Connection.QuerySingleOrDefaultAsync<BuilderApplicationEntity>(SelectBuilderAppByIdProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<BuilderApplicationEntity>.Success(application!);
        }
        catch (DbException ex)
        {
            return Result<BuilderApplicationEntity>.Failure($"Failed to retrieve builder application: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}
