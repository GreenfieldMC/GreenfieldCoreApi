-- DependsOn: ScriptHistory, BuilderApps
create table if not exists BuilderAppImageLinks (
    BuilderAppImageLinkId bigint not null unique auto_increment primary key,
    ApplicationId bigint not null,
    LinkType nvarchar(256) not null,
    ImageLink nvarchar(2048) not null,
    CreatedOn datetime default current_timestamp not null,
    constraint FK_BuilderAppImageLinks_BuilderApps foreign key (ApplicationId) references BuilderApps(ApplicationId) on delete cascade on update cascade
);