-- DependsOn: ScriptHistory, Applications
create procedure if not exists `BuildApps.usp_SelectApplicationsByUser`(
    p_UserId bigint)
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
    where ba.UserId = p_UserId
    order by ba.ApplicationId;
end;
