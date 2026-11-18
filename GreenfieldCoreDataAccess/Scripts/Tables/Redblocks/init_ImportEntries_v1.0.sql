-- DependsOn: ScriptHistory, ImportJobs, Projects, Redblocks
-- One row per legacy redblock processed by an import job. State is:
--   Importing - claimed by a job and being imported
--   Imported  - created, RedblockId is set. Errors holds any follow-up (status, assignment, entity) failures
--   Skipped   - another job already imported (or is importing) this legacy key into the project
--   Failed    - not created, Errors says why
-- Rows are inserted in order by a job, so ImportEntryId is the cursor for reading a job's progress.
create table if not exists `Redblocks.ImportEntries` (
    ImportEntryId bigint not null unique auto_increment primary key,
    ImportJobId char(36) not null,
    LegacyKey varchar(64) not null,
    ProjectId bigint null,
    State varchar(16) not null,
    RedblockId bigint null,
    Errors text null,
    ProcessedOn datetime default current_timestamp not null,
    -- only Importing and Imported rows hold a legacy key, so a legacy key can be imported into a project once, while
    -- any number of Skipped or Failed rows can exist for it
    ClaimedLegacyKey varchar(64) as (if(State in ('Importing', 'Imported'), LegacyKey, null)) stored,
    constraint UQ_ImportEntries_ProjectId_ClaimedLegacyKey unique (ProjectId, ClaimedLegacyKey),
    constraint FK_ImportEntries_ImportJobs foreign key (ImportJobId) references `Redblocks.ImportJobs`(ImportJobId) on delete cascade,
    constraint FK_ImportEntries_Projects foreign key (ProjectId) references `Redblocks.Projects`(ProjectId) on delete cascade,
    constraint FK_ImportEntries_Redblocks foreign key (RedblockId) references `Redblocks.Redblocks`(RedblockId) on delete set null
) character set utf8mb4 collate utf8mb4_unicode_ci;

create index if not exists IX_ImportEntries_ImportJobId_ImportEntryId on `Redblocks.ImportEntries` (ImportJobId, ImportEntryId);
