-- DependsOn: ScriptHistory, Codes
create procedure if not exists `BuildCodes.usp_DeleteCode`(
    p_CodeId bigint)
begin
    delete from `BuildCodes.Codes` where CodeId = p_CodeId;
end;