-- DependsOn: ScriptHistory, BuildCodes
create procedure if not exists usp_UpdateBuildCode(
    p_BuildCodeId bigint,
    p_ListOrder int,
    p_BuildCode nvarchar(4096))
begin
    update BuildCodes
    set
        ListOrder = p_ListOrder,
        BuildCode = p_BuildCode
    where BuildCodeId = p_BuildCodeId;
end;