-- DependsOn: ScriptHistory, Users, UserDiscord
create procedure if not exists usp_SelectAllDiscordAccounts()
begin
    select ud.UserDiscordId, ud.UserId, ud.DiscordSnowflake, ud.DiscordUsername, ud.RefreshToken, ud.AccessToken,
           ud.TokenType, ud.TokenExpiry, ud.Scope, ud.UpdatedOn, ud.CreatedOn
    from UserDiscord ud;
end;

