-- DependsOn: ScriptHistory, BuilderApps
create procedure if not exists usp_SelectBuilderApplicationsByUser(
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
    from BuilderApps ba
    where ba.UserId = p_UserId
    order by ba.ApplicationId asc;
end;

