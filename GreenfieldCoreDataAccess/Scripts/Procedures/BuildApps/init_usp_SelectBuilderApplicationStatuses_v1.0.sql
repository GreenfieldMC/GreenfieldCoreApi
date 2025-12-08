-- DependsOn: ScriptHistory, BuilderAppStatus
create procedure if not exists usp_SelectBuilderApplicationStatuses(
    p_ApplicationId bigint)
begin
    select
        bas.BuilderAppStatusId,
        bas.ApplicationId,
        bas.Status,
        bas.StatusMessage,
        bas.CreatedOn
    from BuilderAppStatus bas
    where bas.ApplicationId = p_ApplicationId
    order by bas.CreatedOn asc, bas.BuilderAppStatusId asc;
end;

