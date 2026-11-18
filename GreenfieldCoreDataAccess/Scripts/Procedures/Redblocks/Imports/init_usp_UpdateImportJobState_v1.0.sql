-- DependsOn: ScriptHistory, ImportJobs
-- Sets StartedOn when the job starts running, and CompletedOn when it completes or fails.
create procedure if not exists `Redblocks.usp_UpdateImportJobState`(
    p_ImportJobId char(36),
    p_State varchar(16),
    p_ErrorMessage varchar(1024))
begin
    update `Redblocks.ImportJobs` j
    set j.State = p_State,
        j.ErrorMessage = p_ErrorMessage,
        j.StartedOn = if(p_State = 'Running', current_timestamp, j.StartedOn),
        j.CompletedOn = if(p_State in ('Completed', 'Failed'), current_timestamp, j.CompletedOn)
    where j.ImportJobId = p_ImportJobId;
end;
