-- DependsOn: ScriptHistory, Users, UserDiscordConnections, DiscordConnections
create procedure if not exists `Users.usp_SelectDiscordConnectionsByUserId`(
    p_UserId bigint
)
begin
    select udc.UserDiscordConnectionId,
           udc.CreatedOn as UserDiscordConnectionCreatedOn,
           udc.UserId,
           dc.DiscordConnectionId, dc.RefreshToken, dc.AccessToken, dc.TokenType, dc.TokenExpiry, dc.Scope,
           dc.DiscordSnowflake, dc.DiscordUsername, dc.UpdatedOn, dc.CreatedOn
    from `Users.UserDiscordConnections` udc
        inner join `Connections.DiscordConnections` dc on dc.DiscordConnectionId = udc.DiscordConnectionId
    where udc.UserId = p_UserId;
end;

