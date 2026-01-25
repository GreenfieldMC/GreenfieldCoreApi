-- DependsOn: ScriptHistory, Users, DiscordConnections
create table if not exists `Users.UserDiscordConnections` (
    UserDiscordConnectionId bigint auto_increment not null primary key unique,
    UserId bigint not null,
    DiscordConnectionId bigint not null,
    UpdatedOn datetime default null on update current_timestamp null,
    CreatedOn datetime default current_timestamp not null,
    constraint UQ_UserDiscordConnections_UserId_DiscordConnectionId unique (UserId, DiscordConnectionId),
    constraint FK_UserDiscordConnections_Users foreign key (UserId) references `Users.Users`(UserId) on delete cascade on update cascade,
    constraint FK_UserDiscordConnections_DiscordConnections foreign key (DiscordConnectionId) references `Connections.DiscordConnections`(DiscordConnectionId) on delete cascade on update cascade
) character set utf8mb4 collate utf8mb4_unicode_ci;