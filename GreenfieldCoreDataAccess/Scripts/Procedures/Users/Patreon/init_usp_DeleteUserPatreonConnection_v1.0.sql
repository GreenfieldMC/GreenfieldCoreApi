-- DependsOn: ScriptHistory, Users, UserPatreonConnections
create procedure if not exists `Users.usp_DeleteUserPatreonConnection`(
    p_UserId bigint,
    p_PatreonConnectionId bigint
)
begin
    delete from `Users.UserPatreonConnections`
    where UserId = p_UserId
      and PatreonConnectionId = p_PatreonConnectionId;
end;

