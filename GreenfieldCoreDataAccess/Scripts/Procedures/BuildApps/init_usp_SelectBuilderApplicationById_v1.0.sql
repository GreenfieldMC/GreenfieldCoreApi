-- DependsOn: ScriptHistory, BuilderApps
create procedure if not exists usp_SelectBuilderApplicationById(
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
    from BuilderApps ba
    where ba.ApplicationId = p_ApplicationId;
end;

