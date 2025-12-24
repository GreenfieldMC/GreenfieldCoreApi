-- DependsOn: ScriptHistory, Users, UserDiscord
create procedure if not exists usp_InsertUserDiscordAccount(
    p_UserId bigint,
    p_DiscordSnowflake bigint unsigned)
begin
    -- Attempt insert; ignore if UserId and DiscordSnowflake combination already exists
    insert ignore into UserDiscord (UserId, DiscordSnowflake)
    values (p_UserId, p_DiscordSnowflake);
           
    -- If an insert happened, return the created UserDiscord row
    if row_count() > 0 then
        select ud.UserDiscordId, ud.UserId, ud.DiscordSnowflake, ud.CreatedOn
        from UserDiscord ud
        where ud.UserId = p_UserId
          and ud.DiscordSnowflake = p_DiscordSnowflake;
    end if;
end;