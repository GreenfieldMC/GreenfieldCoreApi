-- DependsOn: ScriptHistory, ImportEntries
-- Inserts an entry for a legacy redblock.
-- When p_State is Importing, the entry claims the legacy key for the project. If the key is already claimed (imported, or
-- being imported by another job), a Skipped entry is inserted instead, so check the State of the returned row.
create procedure if not exists `Redblocks.usp_InsertImportEntry`(
    p_ImportJobId char(36),
    p_LegacyKey varchar(64),
    p_ProjectId bigint,
    p_State varchar(16),
    p_Errors text)
begin
    declare v_ImportEntryId bigint default null;

    if p_State = 'Importing' then
        insert ignore into `Redblocks.ImportEntries` (
            ImportJobId,
            LegacyKey,
            ProjectId,
            State,
            Errors)
        values (
            p_ImportJobId,
            p_LegacyKey,
            p_ProjectId,
            p_State,
            p_Errors);

        if row_count() > 0 then
            set v_ImportEntryId = last_insert_id();
        else
            set p_State = 'Skipped';
        end if;
    end if;

    if v_ImportEntryId is null then
        insert into `Redblocks.ImportEntries` (
            ImportJobId,
            LegacyKey,
            ProjectId,
            State,
            Errors)
        values (
            p_ImportJobId,
            p_LegacyKey,
            p_ProjectId,
            p_State,
            p_Errors);

        set v_ImportEntryId = last_insert_id();
    end if;

    select
        e.ImportEntryId,
        e.State
    from `Redblocks.ImportEntries` e
    where e.ImportEntryId = v_ImportEntryId;
end;
