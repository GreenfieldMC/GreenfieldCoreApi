-- DependsOn: ScriptHistory, Users, PatreonConnections
create table if not exists `Users.UserPatreonConnections` (
    UserPatreonConnectionId bigint auto_increment not null primary key unique,
    UserId bigint not null,
    PatreonConnectionId bigint not null,
    UpdatedOn datetime default null on update current_timestamp null,
    CreatedOn datetime default current_timestamp not null,
    constraint UQ_UserPatreonConnections_UserId_PatreonConnectionId unique (UserId, PatreonConnectionId),
    constraint FK_UserPatreonConnections_Users foreign key (UserId) references `Users.Users`(UserId) on delete cascade on update cascade,
    constraint FK_UserPatreonConnections_PatreonConnections foreign key (PatreonConnectionId) references `Connections.PatreonConnections`(PatreonConnectionId) on delete cascade on update cascade
) character set utf8mb4 collate utf8mb4_unicode_ci;