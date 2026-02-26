-- DependsOn: ScriptHistory, Applications, ApplicationStatus
create procedure if not exists `BuildApps.usp_SelectApplicationsWithLatestStatusByUser`(
    p_UserId bigint)
begin
    SELECT
        ba.ApplicationId,
        bas.ApplicationStatusId,
        bas.Status,
        bas.StatusMessage,
        bas.CreatedOn
    FROM `BuildApps.Applications` ba
    LEFT JOIN (
        SELECT *, ROW_NUMBER() OVER (PARTITION BY ApplicationId ORDER BY CreatedOn DESC) AS rn
            FROM `BuildApps.ApplicationStatus`
    ) bas ON bas.ApplicationId = ba.ApplicationId AND bas.rn = 1
    WHERE ba.UserId = p_UserId
    ORDER BY bas.CreatedOn DESC;
END
