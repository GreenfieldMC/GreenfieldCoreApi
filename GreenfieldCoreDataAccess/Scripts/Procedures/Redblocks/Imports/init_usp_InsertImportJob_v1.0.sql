-- DependsOn: ScriptHistory, ImportJobs
create procedure if not exists `Redblocks.usp_InsertImportJob`(
    p_ImportJobId char(36),
    p_Total int)
begin
    insert into `Redblocks.ImportJobs` (
        ImportJobId,
        State,
        Total)
    values (
        p_ImportJobId,
        'Queued',
        p_Total);
end;
