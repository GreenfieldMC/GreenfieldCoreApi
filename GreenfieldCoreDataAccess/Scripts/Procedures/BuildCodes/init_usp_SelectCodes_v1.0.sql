-- DependsOn: ScriptHistory, Codes
create procedure if not exists `BuildCodes.usp_SelectCodes`()
begin
    select
        CodeId,
        ListOrder,
        BuildCode,
        CreatedOn
    from `BuildCodes.Codes`
    order by ListOrder, BuildCode;
end;