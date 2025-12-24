-- DependsOn: ScriptHistory, Users, UserDiscord
create procedure if not exists usp_SelectUsersByDiscordSnowflake(
    p_DiscordSnowflake bigint unsigned)
begin
    select u.UserId, u.MinecraftUuid, u.MinecraftUsername, u.CreatedOn
    from Users u
    inner join UserDiscord ud on ud.UserId = u.UserId
    where ud.DiscordSnowflake = p_DiscordSnowflake;
end;

