-- DependsOn: ScriptHistory, Users, UserPatreon
create procedure if not exists usp_UpdateUserPatreonPledge(
    p_UserId bigint,
    p_PatreonId bigint,
    p_NewPledge decimal
) begin
    update UserPatreon
    set Pledge = p_NewPledge
    where UserId = p_UserId and PatreonId = p_PatreonId;
end;