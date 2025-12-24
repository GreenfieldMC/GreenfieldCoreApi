-- DependsOn: ScriptHistory, Users, UserPatreon
create procedure if not exists usp_SelectUserPatreonAccounts(
    p_UserId bigint
) begin
    select up.UserPatreonId, up.UserId, up.RefreshToken, up.AccessToken,
        up.TokenType, up.TokenExpiry, up.Scope, up.PatreonId, up.Pledge, up.UpdatedOn, up.CreatedOn
    from UserPatreon up
    where up.UserId = p_UserId;
end;
