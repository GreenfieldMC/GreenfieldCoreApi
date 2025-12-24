-- DependsOn: ScriptHistory, Users, UserPatreon
create procedure if not exists usp_InsertUserPatreonAccount(
    p_UserId bigint,
    p_RefreshToken nvarchar(512),
    p_AccessToken nvarchar(512),
    p_TokenType nvarchar(128),
    p_TokenExpiry datetime,
    p_Scope nvarchar(1024),
    p_PatreonId bigint,
    p_Pledge decimal
) begin
    insert into UserPatreon (
        UserId,
        RefreshToken,
        AccessToken,
        TokenType,
        TokenExpiry,
        Scope,
        PatreonId,
        Pledge
    ) values (
        p_UserId,
        p_RefreshToken,
        p_AccessToken,
        p_TokenType,
        p_TokenExpiry,
        p_Scope,
        p_PatreonId,
        p_Pledge
    );
    
    if row_count() > 0 then
        select up.UserPatreonId, up.UserId, up.RefreshToken, up.AccessToken,
            up.TokenType, up.TokenExpiry, up.Scope, up.PatreonId, up.Pledge, up.UpdatedOn, up.CreatedOn
        from UserPatreon up
        where up.UserId = p_UserId and up.PatreonId = p_PatreonId;
    end if;
end;