-- DependsOn: ScriptHistory, BuildCodes
create procedure if not exists usp_DeleteBuildCode(
    p_BuildCodeId bigint)
begin
    update BuildCodes
    set Deleted = 1
    where BuildCodeId = p_BuildCodeId;
end;