-- DependsOn: ScriptHistory, ImportJobs, ImportEntries, Redblocks
-- MANUAL USE ONLY, not called by the api. Completely removes an import job: hard deletes every redblock it created
-- (their statuses, assignments and entities cascade), then the job and its entries, which frees the legacy keys so they
-- can be imported again. The blocks and display entities in game are not touched.
-- Project key numbers are not reused, so the next redblock in an affected project continues after the deleted ones.
-- Refuses to wipe a job that is still queued or running.
create procedure if not exists `Redblocks.usp_WipeImportJob`(
    p_ImportJobId char(36))
begin
    declare v_State varchar(16);
    declare v_DeletedRedblocks int;

    declare exit handler for sqlexception
    begin
        rollback;
        resignal;
    end;

    select j.State
    into v_State
    from `Redblocks.ImportJobs` j
    where j.ImportJobId = p_ImportJobId;

    if v_State is null then
        signal sqlstate '45000' set message_text = 'Import job not found.';
    elseif v_State in ('Queued', 'Running') then
        signal sqlstate '45000' set message_text = 'Import job is still queued or running.';
    end if;

    start transaction;

    delete rb
    from `Redblocks.Redblocks` rb
    inner join `Redblocks.ImportEntries` e on e.RedblockId = rb.RedblockId
    where e.ImportJobId = p_ImportJobId;

    set v_DeletedRedblocks = row_count();

    -- entries cascade
    delete from `Redblocks.ImportJobs`
    where ImportJobId = p_ImportJobId;

    commit;

    select v_DeletedRedblocks as DeletedRedblocks;
end;
