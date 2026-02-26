-- DependsOn: ScriptHistory, DiscordConnections
create procedure if not exists `Connections.Discord.usp_SelectDiscordConnection`(
    p_DiscordConnectionId bigint
)
begin
    select dc.DiscordConnectionId, dc.RefreshToken, dc.AccessToken, dc.TokenType, dc.TokenExpiry, dc.Scope,
           dc.DiscordSnowflake, dc.DiscordUsername, dc.UpdatedOn, dc.CreatedOn
    from `Connections.DiscordConnections` dc
    where dc.DiscordConnectionId = p_DiscordConnectionId;
end;

