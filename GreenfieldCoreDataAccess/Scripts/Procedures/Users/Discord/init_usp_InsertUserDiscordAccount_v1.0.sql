-- DependsOn: ScriptHistory, Users, UserDiscord
create procedure if not exists usp_InsertUserDiscordAccount(
    p_UserId bigint,
    p_DiscordSnowflake bigint unsigned,
    p_DiscordUsername nvarchar(256),
    p_RefreshToken nvarchar(512),
    p_AccessToken nvarchar(512),
    p_TokenType nvarchar(64),
    p_TokenExpiry datetime,
    p_Scope nvarchar(1024))
begin
    -- Attempt insert; ignore if UserId and DiscordSnowflake combination already exists
    insert ignore into UserDiscord (UserId, DiscordSnowflake, DiscordUsername, RefreshToken, AccessToken, TokenType, TokenExpiry, Scope)
    values (p_UserId, p_DiscordSnowflake, p_DiscordUsername, p_RefreshToken, p_AccessToken, p_TokenType, p_TokenExpiry, p_Scope);
           
    -- If an insert happened, return the created UserDiscord row
    if row_count() > 0 then
        select ud.UserDiscordId, ud.UserId, ud.DiscordSnowflake, ud.DiscordUsername, ud.RefreshToken, ud.AccessToken,
               ud.TokenType, ud.TokenExpiry, ud.Scope, ud.UpdatedOn, ud.CreatedOn
        from UserDiscord ud
        where ud.UserId = p_UserId
          and ud.DiscordSnowflake = p_DiscordSnowflake;
    end if;
end;