-- DependsOn: ScriptHistory
create table if not exists BuildCodes (
    BuildCodeId bigint not null unique auto_increment primary key,
    ListOrder int not null,
    BuildCode nvarchar(4096) not null,
    Deleted bit not null default 0,
    CreatedOn datetime default current_timestamp not null
);