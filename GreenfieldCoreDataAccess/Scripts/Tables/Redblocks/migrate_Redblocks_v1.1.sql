-- DependsOn: ScriptHistory, Redblocks
-- CurrentStatus mirrors the latest row in Redblocks.Statuses so searches do not have to rank the whole status history.
-- It is kept in sync by the trigger created in migrate_Statuses_v1.1, which also backfills existing redblocks.
alter table `Redblocks.Redblocks` add column if not exists CurrentStatus varchar(32) null after Message;

create index if not exists IX_Redblocks_ProjectId_CurrentStatus on `Redblocks.Redblocks` (ProjectId, CurrentStatus);
create index if not exists IX_Redblocks_ProjectId_DeletedBy on `Redblocks.Redblocks` (ProjectId, DeletedBy);

