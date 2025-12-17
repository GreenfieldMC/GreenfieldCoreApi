-- DependsOn: ScriptHistory, BuilderAppStatus
create procedure if not exists usp_SelectBuilderApplicationsWithLatestStatusByUser(
    p_UserId bigint)
begin
    SELECT
        ba.ApplicationId,
        bas.BuilderAppStatusId,
        bas.Status,
        bas.StatusMessage,
        bas.CreatedOn
    FROM BuilderApps ba
    LEFT JOIN (
        SELECT *, ROW_NUMBER() OVER (PARTITION BY ApplicationId ORDER BY CreatedOn DESC) AS rn
            FROM BuilderAppStatus
    ) bas ON bas.ApplicationId = ba.ApplicationId AND bas.rn = 1
    WHERE ba.UserId = p_UserId
    ORDER BY bas.CreatedOn DESC;
END

