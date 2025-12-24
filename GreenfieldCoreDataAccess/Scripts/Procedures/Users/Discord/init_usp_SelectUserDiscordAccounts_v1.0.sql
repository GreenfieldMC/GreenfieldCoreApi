-- DependsOn: ScriptHistory, Users, UserDiscord
create procedure if not exists usp_SelectUserDiscordAccounts(
    p_UserId bigint)
begin
    select ud.UserDiscordId, ud.UserId, ud.DiscordSnowflake, ud.CreatedOn
    from UserDiscord ud
    where ud.UserId = p_UserId;
end;