-- DependsOn: ScriptHistory, Clients
create procedure if not exists `Clients.usp_DeleteClient`(
    p_ClientId char(36))
begin
    delete from Clients.Clients where ClientId = p_ClientId;
end;


