-- DependsOn: ScriptHistory, Users, UserPatreon
create procedure if not exists usp_SelectUserPatreonAccount(
    p_UserId bigint,
    p_PatreonId bigint
) begin
    select up.UserPatreonId, up.UserId, up.RefreshToken, up.AccessToken,
        up.TokenType, up.TokenExpiry, up.Scope, up.PatreonId, up.Pledge, up.FullName, up.UpdatedOn, up.CreatedOn
    from UserPatreon up
    where up.UserId = p_UserId
      and up.PatreonId = p_PatreonId;
end;