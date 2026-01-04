-- DependsOn: ScriptHistory, PatreonConnections
create procedure if not exists `Connections.Patreon.usp_SelectAllPatreonConnections`()
begin
    select pc.PatreonConnectionId, pc.RefreshToken, pc.AccessToken, pc.TokenType, pc.TokenExpiry, pc.Scope,
           pc.PatreonId, pc.FullName, pc.Pledge, pc.UpdatedOn, pc.CreatedOn
    from `Connections.PatreonConnections` pc;
end;

