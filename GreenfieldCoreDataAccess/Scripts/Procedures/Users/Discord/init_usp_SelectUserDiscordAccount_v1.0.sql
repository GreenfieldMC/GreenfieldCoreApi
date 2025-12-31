-- DependsOn: ScriptHistory, Users, UserDiscord
create procedure if not exists usp_SelectUserDiscordAccount(
    p_UserId bigint,
    p_DiscordSnowflake bigint
) begin
    select ud.UserDiscordId, ud.UserId, ud.DiscordSnowflake, ud.DiscordUsername, ud.RefreshToken, ud.AccessToken,
           ud.TokenType, ud.TokenExpiry, ud.Scope, ud.UpdatedOn, ud.CreatedOn
    from UserDiscord ud
    where ud.UserId = p_UserId
      and ud.DiscordSnowflake = p_DiscordSnowflake;
end;