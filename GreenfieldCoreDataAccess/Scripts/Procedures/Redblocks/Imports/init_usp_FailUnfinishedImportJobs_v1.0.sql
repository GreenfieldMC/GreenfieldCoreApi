-- DependsOn: ScriptHistory, ImportJobs
-- Import payloads are only held in memory, so jobs that were queued or running when the api stopped can never finish.
-- This is run on startup to mark them as failed.
create procedure if not exists `Redblocks.usp_FailUnfinishedImportJobs`(
    p_ErrorMessage varchar(1024))
begin
    update `Redblocks.ImportJobs` j
    set j.State = 'Failed',
        j.ErrorMessage = p_ErrorMessage,
        j.CompletedOn = current_timestamp
    where j.State in ('Queued', 'Running');

    select row_count();
end;
