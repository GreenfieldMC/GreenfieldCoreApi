-- DependsOn: ScriptHistory, DiscordConnections
create procedure if not exists `Connections.Discord.usp_SelectDiscordConnectionBySnowflake`(
    p_DiscordSnowflake bigint unsigned
)
begin
    select dc.DiscordConnectionId, dc.RefreshToken, dc.AccessToken, dc.TokenType, dc.TokenExpiry, dc.Scope,
           dc.DiscordSnowflake, dc.DiscordUsername, dc.UpdatedOn, dc.CreatedOn
    from `Connections.DiscordConnections` dc
    where dc.DiscordSnowflake = p_DiscordSnowflake;
end;

