-- DependsOn: ScriptHistory, ClientRoles
create procedure if not exists `Clients.usp_ClearClientRoles`(
    p_ClientId char(36))
begin
    delete from `Clients.ClientRoles` where ClientId = p_ClientId;
end;