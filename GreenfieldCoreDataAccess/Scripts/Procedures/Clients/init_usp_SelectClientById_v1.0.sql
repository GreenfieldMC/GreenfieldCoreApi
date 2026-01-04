-- DependsOn: ScriptHistory, Clients
create procedure if not exists `Clients.usp_SelectClientById`(
    p_ClientId char(36))
begin
    select c.ClientId, c.ClientName, c.Salt, c.CreatedOn
    from `Clients.Clients` c
    where c.ClientId = p_ClientId;
end;
