-- DependsOn: ScriptHistory, Users
create procedure if not exists `Users.usp_SelectUserByUuid`(
    p_MinecraftUuid char(36))
begin
select u.UserId, u.MinecraftUuid, u.MinecraftUsername, u.CreatedOn
from `Users.Users` u
where u.MinecraftUuid = p_MinecraftUuid;
end;