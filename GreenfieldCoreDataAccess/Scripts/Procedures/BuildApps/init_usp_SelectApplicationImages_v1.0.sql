-- DependsOn: ScriptHistory, ImageLinks
create procedure if not exists `BuildApps.usp_SelectApplicationImages`(
    p_ApplicationId bigint)
begin
    select
        ail.ImageLinkId,
        ail.ApplicationId,
        ail.LinkType,
        ail.ImageLink,
        ail.UpdatedOn,
        ail.CreatedOn
    from `BuildApps.ImageLinks` ail
    where ail.ApplicationId = p_ApplicationId
    order by ail.CreatedOn, ail.ImageLinkId;
end;
