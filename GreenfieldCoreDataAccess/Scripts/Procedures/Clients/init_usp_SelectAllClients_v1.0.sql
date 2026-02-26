-- DependsOn: ScriptHistory, Clients
create procedure if not exists `Clients.usp_SelectAllClients`()
begin
    select c.ClientId, c.ClientName, c.Salt, c.CreatedOn
    from `Clients.Clients` c;
end;
