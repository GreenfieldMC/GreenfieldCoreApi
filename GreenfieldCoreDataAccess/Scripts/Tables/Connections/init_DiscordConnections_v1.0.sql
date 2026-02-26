-- DependsOn: ScriptHistory
create table if not exists `Connections.DiscordConnections` (
   DiscordConnectionId bigint auto_increment not null primary key unique,
   RefreshToken nvarchar(512) not null,
   AccessToken nvarchar(512) not null,
   TokenType nvarchar(64) not null,
   TokenExpiry datetime not null,
   Scope nvarchar(1024) not null,
   DiscordSnowflake bigint unsigned not null,
   DiscordUsername nvarchar(256) null,
   UpdatedOn datetime default null on update current_timestamp null,
   CreatedOn datetime default current_timestamp not null,
   constraint UQ_DiscordConnections_DiscordSnowflake unique (DiscordSnowflake)
) character set utf8mb4 collate utf8mb4_unicode_ci;