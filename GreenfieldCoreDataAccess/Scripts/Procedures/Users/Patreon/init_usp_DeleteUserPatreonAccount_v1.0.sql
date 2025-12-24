-- DependsOn: ScriptHistory, Users, UserPatreon
create procedure if not exists usp_DeleteUserPatreonAccount(
    p_UserId bigint,
    p_PatreonId bigint)
begin
    delete from UserPatreon
    where UserId = p_UserId
      and PatreonId = p_PatreonId;
end;