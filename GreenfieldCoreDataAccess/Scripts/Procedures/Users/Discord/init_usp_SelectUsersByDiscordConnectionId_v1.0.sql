-- DependsOn: ScriptHistory, Users, UserDiscordConnections, DiscordConnections
create procedure if not exists `Users.usp_SelectUsersByDiscordConnectionId`(
    p_DiscordConnectionId bigint
)
begin
    select udc.UserDiscordConnectionId,
           udc.CreatedOn as UserDiscordConnectionCreatedOn,
           u.UserId, u.MinecraftUuid, u.MinecraftUsername, u.CreatedOn
    from `Users.UserDiscordConnections` udc
        inner join `Users.Users` u on u.UserId = udc.UserId
    where udc.DiscordConnectionId = p_DiscordConnectionId;
end;

