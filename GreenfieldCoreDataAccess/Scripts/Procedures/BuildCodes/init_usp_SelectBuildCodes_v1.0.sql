-- DependsOn: ScriptHistory, BuildCodes
create procedure if not exists usp_SelectBuildCodes()
begin
    select
        BuildCodeId,
        ListOrder,
        BuildCode,
        Deleted,
        CreatedOn
    from BuildCodes
    order by ListOrder, BuildCode;
end;