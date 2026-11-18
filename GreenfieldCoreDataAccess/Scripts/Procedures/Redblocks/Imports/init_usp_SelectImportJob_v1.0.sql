-- DependsOn: ScriptHistory, ImportJobs, ImportEntries
create procedure if not exists `Redblocks.usp_SelectImportJob`(
    p_ImportJobId char(36))
begin
    select
        j.ImportJobId,
        j.State,
        j.Total,
        j.ErrorMessage,
        j.CreatedOn,
        j.StartedOn,
        j.CompletedOn,
        (select count(*) from `Redblocks.ImportEntries` e where e.ImportJobId = j.ImportJobId and e.State <> 'Importing') as Processed,
        (select count(*) from `Redblocks.ImportEntries` e where e.ImportJobId = j.ImportJobId and e.State = 'Failed') as Failed
    from `Redblocks.ImportJobs` j
    where j.ImportJobId = p_ImportJobId;
end;
