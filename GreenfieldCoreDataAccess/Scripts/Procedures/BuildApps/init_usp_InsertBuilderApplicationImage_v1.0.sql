-- DependsOn: ScriptHistory, BuilderAppImageLinks
create procedure if not exists usp_InsertBuilderApplicationImage(
    p_ApplicationId bigint,
    p_LinkType nvarchar(256),
    p_ImageLink nvarchar(2048))
begin
    insert into BuilderAppImageLinks (
        ApplicationId,
        LinkType,
        ImageLink)
    values (
        p_ApplicationId,
        p_LinkType,
        p_ImageLink);

    select 
        bali.BuilderAppImageLinkId,
        bali.ApplicationId,
        bali.LinkType,
        bali.ImageLink,
        bali.CreatedOn
    from BuilderAppImageLinks bali
    where bali.BuilderAppImageLinkId = last_insert_id();
end;

