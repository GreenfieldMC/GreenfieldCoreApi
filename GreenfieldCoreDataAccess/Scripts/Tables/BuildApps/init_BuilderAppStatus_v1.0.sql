-- DependsOn: ScriptHistory, BuilderApps
create table if not exists BuilderAppStatus (
    BuilderAppStatusId bigint not null unique auto_increment primary key,
    ApplicationId bigint not null,
    Status nvarchar(256) not null,
    StatusMessage nvarchar(4096) null,
    CreatedOn datetime default current_timestamp not null,
    constraint FK_BuilderAppStatus_BuilderApps foreign key (ApplicationId) references BuilderApps(ApplicationId) on delete cascade on update cascade
)