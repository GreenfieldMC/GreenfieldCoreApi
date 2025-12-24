-- DependsOn: ScriptHistory, Users, UserPatreon
create procedure if not exists usp_UpdateUserPatreonTokens(
    p_UserId bigint,
    p_RefreshToken nvarchar(512),
    p_AccessToken nvarchar(512),
    p_TokenType nvarchar(64),
    p_TokenExpiry datetime,
    p_Scope nvarchar(1024),
    p_PatreonId bigint
) begin
    update UserPatreon
    set RefreshToken = p_RefreshToken,
        AccessToken = p_AccessToken,
        TokenType = p_TokenType,
        TokenExpiry = p_TokenExpiry,
        Scope = p_Scope
    where UserId = p_UserId and PatreonId = p_PatreonId;
end;
