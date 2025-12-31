-- DependsOn: ScriptHistory, Users
create table if not exists UserDiscord (
    UserDiscordId bigint auto_increment not null primary key unique,
    UserId bigint not null,
    DiscordSnowflake bigint unsigned not null,
    DiscordUsername nvarchar(256) null,
    RefreshToken nvarchar(512) not null,
    AccessToken nvarchar(512) not null,
    TokenType nvarchar(64) not null,
    TokenExpiry datetime not null,
    Scope nvarchar(1024) not null,
    UpdatedOn datetime default null on update current_timestamp null,
    CreatedOn datetime default current_timestamp not null,
    constraint UQ_UserDiscord_UserId_DiscordUserId unique (UserId, DiscordSnowflake),
    constraint FK_UserDiscord_Users foreign key (UserId) references Users(UserId) on delete cascade on update cascade
) character set utf8mb4 collate utf8mb4_unicode_ci;
