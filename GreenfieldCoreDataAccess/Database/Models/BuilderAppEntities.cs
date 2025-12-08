using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Models;

public record BuilderApplicationEntity(
    long ApplicationId,
    long UserId,
    int UserAge,
    string UserNationality,
    string? AdditionalBuildingInformation,
    string WhyJoinGreenfield,
    string? AdditionalComments,
    DateTime CreatedOn);

public record BuilderAppImageLinkEntity(
    long BuilderAppImageLinkId,
    long ApplicationId,
    string LinkType,
    string ImageLink,
    DateTime CreatedOn);

public record BuilderAppStatusEntity(
    long BuilderAppStatusId,
    long ApplicationId,
    string Status,
    string? StatusMessage,
    DateTime CreatedOn);

public record BuilderApplicationInsertResult(
    BuilderApplicationEntity Application,
    IReadOnlyList<Result<string>> FailedImageLinks)
{
    public static BuilderApplicationInsertResult From(BuilderApplicationEntity application, IEnumerable<Result<string>> failedImages)
        => new(application, failedImages.ToList());
}
