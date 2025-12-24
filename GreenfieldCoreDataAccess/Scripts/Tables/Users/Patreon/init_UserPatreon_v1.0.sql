-- DependsOn: ScriptHistory, Users
create table if not exists UserPatreon (
    UserPatreonId bigint auto_increment not null primary key unique,
    UserId bigint not null,
    RefreshToken nvarchar(512) character set utf8mb4 collate utf8mb4_unicode_ci not null,
    AccessToken nvarchar(512) character set utf8mb4 collate utf8mb4_unicode_ci not null,
    TokenType nvarchar(64) character set utf8mb4 collate utf8mb4_unicode_ci not null,
    TokenExpiry datetime not null,
    Scope nvarchar(1024) character set utf8mb4 collate utf8mb4_unicode_ci not null,
    PatreonId bigint not null,
    Pledge decimal null,
    UpdatedOn datetime default current_timestamp on update current_timestamp not null,
    CreatedOn datetime default current_timestamp not null,
    constraint UQ_UserPatreon_UserId unique (UserId),
    constraint FK_UserPatreon_Users foreign key (UserId) references Users(UserId) on delete cascade on update cascade
)