-- DependsOn: ScriptHistory, ImportEntries
-- Finishes an Importing entry, setting it to Imported (with the created redblock) or Failed.
create procedure if not exists `Redblocks.usp_UpdateImportEntry`(
    p_ImportEntryId bigint,
    p_State varchar(16),
    p_RedblockId bigint,
    p_Errors text)
begin
    update `Redblocks.ImportEntries` e
    set e.State = p_State,
        e.RedblockId = p_RedblockId,
        e.Errors = p_Errors,
        e.ProcessedOn = current_timestamp
    where e.ImportEntryId = p_ImportEntryId;
end;
