-- DependsOn: ScriptHistory, PatreonConnections
create procedure if not exists `Connections.Patreon.usp_DeletePatreonConnection`(
    p_PatreonConnectionId bigint
)
begin
    delete from `Connections.PatreonConnections`
    where PatreonConnectionId = p_PatreonConnectionId;
end;

