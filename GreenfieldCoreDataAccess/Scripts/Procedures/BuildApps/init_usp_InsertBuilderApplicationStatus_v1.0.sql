-- DependsOn: ScriptHistory, BuilderAppStatus
create procedure if not exists usp_InsertBuilderApplicationStatus(
    p_ApplicationId bigint,
    p_Status nvarchar(256),
    p_StatusMessage nvarchar(2048))
begin
    insert into BuilderAppStatus (
        ApplicationId,
        Status,
        StatusMessage)
    values (
        p_ApplicationId,
        p_Status,
        p_StatusMessage);
end;

