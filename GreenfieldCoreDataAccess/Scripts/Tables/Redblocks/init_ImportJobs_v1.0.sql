-- DependsOn: ScriptHistory
-- One row per bulk import of legacy redblocks. State is Queued, Running, Completed or Failed.
-- ErrorMessage is only set when the whole job failed, e.g. the system user was missing or the api restarted mid-import.
create table if not exists `Redblocks.ImportJobs` (
    ImportJobId char(36) not null primary key,
    State varchar(16) not null,
    Total int not null,
    ErrorMessage varchar(1024) null,
    CreatedOn datetime default current_timestamp not null,
    StartedOn datetime null,
    CompletedOn datetime null
) character set utf8mb4 collate utf8mb4_unicode_ci;

create index if not exists IX_ImportJobs_State on `Redblocks.ImportJobs` (State);
