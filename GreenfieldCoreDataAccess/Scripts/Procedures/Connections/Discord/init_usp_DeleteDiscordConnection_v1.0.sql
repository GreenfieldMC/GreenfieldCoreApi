-- DependsOn: ScriptHistory, DiscordConnections
create procedure if not exists `Connections.Discord.usp_DeleteDiscordConnection`(
    p_DiscordConnectionId bigint
)
begin
    delete from `Connections.DiscordConnections`
    where DiscordConnectionId = p_DiscordConnectionId;
end;

