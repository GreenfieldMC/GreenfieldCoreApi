namespace GreenfieldCoreDataAccess.Database.Models;

public record ApplicationEntity(
    long ApplicationId,
    long UserId,
    int UserAge,
    string UserNationality,
    string? AdditionalBuildingInformation,
    string WhyJoinGreenfield,
    string? AdditionalComments,
    DateTime CreatedOn);

public record ApplicationImageLinkEntity(
    long ImageLinkId,
    long ApplicationId,
    string LinkType,
    string ImageLink,
    DateTime? UpdatedOn,
    DateTime CreatedOn);

public record ApplicationStatusEntity(
    long ApplicationStatusId,
    long ApplicationId,
    string Status,
    string? StatusMessage,
    DateTime CreatedOn);
    
public record LatestApplicationStatusEntity(
    long ApplicationId,
    long? ApplicationStatusId,
    string? Status,
    string? StatusMessage,
    DateTime? CreatedOn);