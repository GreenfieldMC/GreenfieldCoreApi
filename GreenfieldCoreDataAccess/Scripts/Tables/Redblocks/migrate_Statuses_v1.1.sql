-- DependsOn: ScriptHistory, Redblocks, Statuses
drop trigger if exists `Redblocks.trg_StatusesAfterInsert_SetRedblocksCurrentStatus`;
create trigger `Redblocks.trg_StatusesAfterInsert_SetRedblocksCurrentStatus`
after insert on `Redblocks.Statuses`
for each row
update `Redblocks.Redblocks` rb
set rb.CurrentStatus = new.Status
where rb.RedblockId = new.RedblockId;

-- backfill existing redblocks with their latest (highest StatusId) status
update `Redblocks.Redblocks` rb
inner join `Redblocks.Statuses` rs on rs.StatusId = (
    select max(rs_latest.StatusId)
    from `Redblocks.Statuses` rs_latest
    where rs_latest.RedblockId = rb.RedblockId
)
set rb.CurrentStatus = rs.Status;

