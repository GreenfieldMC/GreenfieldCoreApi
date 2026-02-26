-- DependsOn: ScriptHistory, Users, UserDiscordConnections, DiscordConnections
create procedure if not exists `Users.usp_InsertUserDiscordConnection`(
    p_UserId bigint,
    p_DiscordConnectionId bigint
)
begin
    insert ignore into `Users.UserDiscordConnections` (UserId, DiscordConnectionId)
    values (p_UserId, p_DiscordConnectionId);

    if row_count() > 0 then
        select udc.UserDiscordConnectionId,
               udc.CreatedOn as UserDiscordConnectionCreatedOn,
               udc.UserId,
               dc.DiscordConnectionId, dc.RefreshToken, dc.AccessToken, dc.TokenType, dc.TokenExpiry, dc.Scope,
               dc.DiscordSnowflake, dc.DiscordUsername, dc.UpdatedOn, dc.CreatedOn
        from `Users.UserDiscordConnections` udc
            inner join `Connections.DiscordConnections` dc on dc.DiscordConnectionId = udc.DiscordConnectionId
        where udc.UserId = p_UserId
          and udc.DiscordConnectionId = p_DiscordConnectionId;
    end if;
end;

