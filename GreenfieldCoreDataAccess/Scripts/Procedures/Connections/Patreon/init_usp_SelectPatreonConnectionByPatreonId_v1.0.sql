-- DependsOn: ScriptHistory, PatreonConnections
create procedure if not exists `Connections.Patreon.usp_SelectPatreonConnectionByPatreonId`(
    p_PatreonId bigint
)
begin
    select pc.PatreonConnectionId, pc.RefreshToken, pc.AccessToken, pc.TokenType, pc.TokenExpiry, pc.Scope,
           pc.PatreonId, pc.FullName, pc.Pledge, pc.UpdatedOn, pc.CreatedOn
    from `Connections.PatreonConnections` pc
    where pc.PatreonId = p_PatreonId;
end;

