-- DependsOn: ScriptHistory, Users, UserDiscord
create procedure if not exists usp_DeleteUserDiscordAccount(
    p_UserId bigint,
    p_DiscordSnowflake bigint)
begin
    delete from UserDiscord
    where UserId = p_UserId
      and DiscordSnowflake = p_DiscordSnowflake;
end;