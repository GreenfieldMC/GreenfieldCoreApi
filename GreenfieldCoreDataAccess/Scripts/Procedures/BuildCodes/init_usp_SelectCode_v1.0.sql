-- DependsOn: ScriptHistory, Codes
create procedure if not exists `BuildCodes.usp_SelectCode`(
    p_CodeId bigint)
begin
    select
        CodeId,
        ListOrder,
        BuildCode,
        CreatedOn
    from `BuildCodes.Codes`
    where CodeId = p_CodeId;
end;