-- DependsOn: ScriptHistory, Users, UserPatreonConnections, PatreonConnections
create procedure if not exists `Users.usp_SelectPatreonConnectionsByUserId`(
    p_UserId bigint
)
begin
    select upc.UserPatreonConnectionId,
           upc.CreatedOn as UserPatreonConnectionCreatedOn,
           upc.UserId,
           pc.PatreonConnectionId, pc.RefreshToken, pc.AccessToken, pc.TokenType, pc.TokenExpiry, pc.Scope,
           pc.PatreonId, pc.FullName, pc.Pledge, pc.UpdatedOn, pc.CreatedOn
    from `Users.UserPatreonConnections` upc
        inner join `Connections.PatreonConnections` pc on pc.PatreonConnectionId = upc.PatreonConnectionId
    where upc.UserId = p_UserId;
end;

