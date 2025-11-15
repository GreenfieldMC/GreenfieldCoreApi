-- DependsOn: ScriptHistory, BuildCodes
create procedure if not exists usp_SelectBuildCode(
    p_BuildCodeId bigint)
begin
    select
        BuildCodeId,
        ListOrder,
        BuildCode,
        Deleted,
        CreatedOn
    from BuildCodes
    where BuildCodeId = p_BuildCodeId;
end;