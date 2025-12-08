-- DependsOn: ScriptHistory, Users
create table if not exists BuilderApps (
    ApplicationId bigint not null unique auto_increment primary key,
    UserId bigint not null,
    UserAge int not null,
    UserNationality nvarchar(128) null,
    AdditionalBuildingInformation nvarchar(4096) null,
    WhyJoinGreenfield nvarchar(4096) not null,
    AdditionalComments nvarchar(4096) null,
    CreatedOn datetime default current_timestamp not null,
    constraint FK_BuilderApps_Users foreign key (UserId) references Users(UserId) on delete cascade on update cascade
)
