-- DependsOn: ScriptHistory, Users, UserPatreonConnections, PatreonConnections
create procedure if not exists `Users.usp_SelectUsersByPatreonConnectionId`(
    p_PatreonConnectionId bigint
)
begin
    select upc.UserPatreonConnectionId,
           upc.CreatedOn as UserPatreonConnectionCreatedOn,
           u.UserId, u.MinecraftUuid, u.MinecraftUsername, u.CreatedOn
    from `Users.UserPatreonConnections` upc
        inner join `Users.Users` u on u.UserId = upc.UserId
    where upc.PatreonConnectionId = p_PatreonConnectionId;
end;

