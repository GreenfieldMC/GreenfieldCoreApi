-- DependsOn: ScriptHistory, Users, UserPatreonConnections, PatreonConnections
create procedure if not exists `Users.usp_InsertUserPatreonConnection`(
    p_UserId bigint,
    p_PatreonConnectionId bigint
)
begin
    insert ignore into `Users.UserPatreonConnections` (UserId, PatreonConnectionId)
    values (p_UserId, p_PatreonConnectionId);

    if row_count() > 0 then
        select upc.UserPatreonConnectionId,
               upc.CreatedOn as UserPatreonConnectionCreatedOn,
               upc.UserId,
               pc.PatreonConnectionId, pc.RefreshToken, pc.AccessToken, pc.TokenType, pc.TokenExpiry, pc.Scope,
               pc.PatreonId, pc.FullName, pc.Pledge, pc.UpdatedOn, pc.CreatedOn
        from `Users.UserPatreonConnections` upc
            inner join `Connections.PatreonConnections` pc on pc.PatreonConnectionId = upc.PatreonConnectionId
        where upc.UserId = p_UserId
          and upc.PatreonConnectionId = p_PatreonConnectionId;
    end if;
end;

