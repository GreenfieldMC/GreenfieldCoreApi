-- DependsOn: ScriptHistory, Applications
create procedure if not exists `BuildApps.usp_SelectApplicationById`(
    p_ApplicationId bigint)
begin
    select
        ba.ApplicationId,
        ba.UserId,
        ba.UserAge,
        ba.UserNationality,
        ba.AdditionalBuildingInformation,
        ba.WhyJoinGreenfield,
        ba.AdditionalComments,
        ba.CreatedOn
    from `BuildApps.Applications` ba
    where ba.ApplicationId = p_ApplicationId;
end;
