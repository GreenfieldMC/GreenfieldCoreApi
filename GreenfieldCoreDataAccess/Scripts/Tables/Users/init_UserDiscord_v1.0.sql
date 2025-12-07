-- DependsOn: ScriptHistory, Users
create table if not exists UserDiscord (
    UserDiscordId bigint auto_increment not null primary key unique,
    UserId bigint not null,
    DiscordSnowflake bigint not null,
    CreatedOn datetime default current_timestamp not null,
    constraint UQ_UserDiscord_UserId_DiscordUserId unique (UserId, DiscordSnowflake),
    constraint FK_UserDiscord_Users foreign key (UserId) references Users(UserId) on delete cascade on update cascade
);