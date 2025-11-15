-- DependsOn: ScriptHistory, BuildCodes
create procedure if not exists usp_InsertBuildCode(
    p_ListOrder int,
    p_BuildCode nvarchar(4096))
begin
    insert into BuildCodes (
        ListOrder,
        BuildCode)
    values (
        p_ListOrder,
        p_BuildCode);
    
    if row_count() > 0 then
        select
            bc.BuildCodeId,
            bc.ListOrder,
            bc.BuildCode,
            bc.Deleted,
            bc.CreatedOn
        from BuildCodes bc
        where bc.BuildCodeId = last_insert_id();
    end if;
end;