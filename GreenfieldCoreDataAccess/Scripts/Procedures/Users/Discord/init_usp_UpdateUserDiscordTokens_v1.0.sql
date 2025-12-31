-- DependsOn: ScriptHistory, Users, UserDiscord
create procedure if not exists usp_UpdateUserDiscordTokens(
    p_UserId bigint,
    p_DiscordSnowflake bigint unsigned,
    p_RefreshToken nvarchar(512),
    p_AccessToken nvarchar(512),
    p_TokenType nvarchar(64),
    p_TokenExpiry datetime,
    p_Scope nvarchar(1024))
begin
    update UserDiscord
    set RefreshToken = p_RefreshToken,
        AccessToken = p_AccessToken,
        TokenType = p_TokenType,
        TokenExpiry = p_TokenExpiry,
        Scope = p_Scope
    where UserId = p_UserId
      and DiscordSnowflake = p_DiscordSnowflake;
end;
