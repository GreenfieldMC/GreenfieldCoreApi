-- DependsOn: ScriptHistory, Users, UserDiscord
create procedure if not exists usp_UpdateUserDiscordProfile(
    p_UserId bigint,
    p_DiscordSnowflake bigint unsigned,
    p_DiscordUsername nvarchar(256))
begin
    update UserDiscord
    set DiscordUsername = p_DiscordUsername
    where UserId = p_UserId
      and DiscordSnowflake = p_DiscordSnowflake;
end;

