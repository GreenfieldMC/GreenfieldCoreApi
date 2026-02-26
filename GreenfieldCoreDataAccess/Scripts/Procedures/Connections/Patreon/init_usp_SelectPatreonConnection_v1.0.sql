-- DependsOn: ScriptHistory, PatreonConnections
create procedure if not exists `Connections.Patreon.usp_SelectPatreonConnection`(
    p_PatreonConnectionId bigint
)
begin
    select pc.PatreonConnectionId, pc.RefreshToken, pc.AccessToken, pc.TokenType, pc.TokenExpiry, pc.Scope,
           pc.PatreonId, pc.FullName, pc.Pledge, pc.UpdatedOn, pc.CreatedOn
    from `Connections.PatreonConnections` pc
    where pc.PatreonConnectionId = p_PatreonConnectionId;
end;

