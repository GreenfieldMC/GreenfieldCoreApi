-- DependsOn: ScriptHistory, Users, UserDiscordConnections
create procedure if not exists `Users.usp_DeleteUserDiscordConnection`(
    p_UserId bigint,
    p_DiscordConnectionId bigint
)
begin
    delete from `Users.UserDiscordConnections`
    where UserId = p_UserId
      and DiscordConnectionId = p_DiscordConnectionId;
end;

