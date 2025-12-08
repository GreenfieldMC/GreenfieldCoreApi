-- DependsOn: ScriptHistory, BuilderAppImageLinks
create procedure if not exists usp_SelectBuilderApplicationImages(
    p_ApplicationId bigint)
begin
    select
        bail.BuilderAppImageLinkId,
        bail.ApplicationId,
        bail.LinkType,
        bail.ImageLink,
        bail.CreatedOn
    from BuilderAppImageLinks bail
    where bail.ApplicationId = p_ApplicationId
    order by bail.CreatedOn asc, bail.BuilderAppImageLinkId asc;
end;

