-- DependsOn: ScriptHistory, ImportEntries, Redblocks, Projects
-- Selects a job's entries after the given entry id, in the order they were processed.
-- RedblockKey is the PROJECT-NUMBER key of the created redblock, or null if none was created.
create procedure if not exists `Redblocks.usp_SelectImportEntries`(
    p_ImportJobId char(36),
    p_AfterImportEntryId bigint,
    p_Limit int)
begin
    select
        e.ImportEntryId,
        e.ImportJobId,
        e.LegacyKey,
        e.ProjectId,
        e.State,
        e.RedblockId,
        e.Errors,
        e.ProcessedOn,
        if(rb.RedblockId is null, null, concat(p.ProjectKey, '-', rb.KeyNumber)) as RedblockKey
    from `Redblocks.ImportEntries` e
    left join `Redblocks.Redblocks` rb on rb.RedblockId = e.RedblockId
    left join `Redblocks.Projects` p on p.ProjectId = rb.ProjectId
    where e.ImportJobId = p_ImportJobId
      and e.ImportEntryId > p_AfterImportEntryId
    order by e.ImportEntryId
    limit p_Limit;
end;
