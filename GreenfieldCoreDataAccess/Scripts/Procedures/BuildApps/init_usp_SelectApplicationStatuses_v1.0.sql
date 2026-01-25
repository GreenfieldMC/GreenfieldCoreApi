-- DependsOn: ScriptHistory, ApplicationStatus
create procedure if not exists `BuildApps.usp_SelectApplicationStatuses`(
    p_ApplicationId bigint)
begin
    select
        bas.ApplicationStatusId,
        bas.ApplicationId,
        bas.Status,
        bas.StatusMessage,
        bas.CreatedOn
    from `BuildApps.ApplicationStatus` bas
    where bas.ApplicationId = p_ApplicationId
    order by bas.CreatedOn, bas.ApplicationStatusId;
end;
