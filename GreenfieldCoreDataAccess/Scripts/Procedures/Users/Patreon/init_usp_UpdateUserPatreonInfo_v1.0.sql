-- DependsOn: ScriptHistory, Users, UserPatreon
create procedure if not exists usp_UpdateUserPatreonInfo(
    p_UserId bigint,
    p_PatreonId bigint,
    p_FullName nvarchar(256),
    p_NewPledge decimal
) begin
    update UserPatreon
    set Pledge = p_NewPledge, FullName = p_FullName
    where UserId = p_UserId and PatreonId = p_PatreonId;
end;