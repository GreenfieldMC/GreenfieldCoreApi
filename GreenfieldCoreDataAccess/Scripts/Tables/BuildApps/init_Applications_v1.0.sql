-- DependsOn: ScriptHistory, Users
create table if not exists `BuildApps.Applications` (
    ApplicationId bigint not null unique auto_increment primary key,
    UserId bigint not null,
    UserAge int not null,
    UserNationality nvarchar(128) null,
    AdditionalBuildingInformation nvarchar(4096) null,
    WhyJoinGreenfield nvarchar(4096) not null,
    AdditionalComments nvarchar(4096) null,
    CreatedOn datetime default current_timestamp not null,
    constraint FK_Applications_Users foreign key (UserId) references `Users.Users`(UserId) on delete cascade on update cascade
) character set utf8mb4 collate utf8mb4_unicode_ci;
