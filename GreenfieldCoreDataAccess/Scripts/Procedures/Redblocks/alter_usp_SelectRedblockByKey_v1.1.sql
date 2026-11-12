-- DependsOn: ScriptHistory, Projects, Redblocks
drop procedure if exists `Redblocks.usp_SelectRedblockByKey`;
create procedure if not exists `Redblocks.usp_SelectRedblockByKey`(
    p_ProjectId bigint,
    p_KeyNumber bigint)
begin
    select
        rb.RedblockId,
        rb.ProjectId,
        rb.KeyNumber,
        rb.Message,
        rb.CurrentStatus as Status,
        rb.X,
        rb.Y,
        rb.Z,
        rb.CreatedBy,
        rb.CreatedOn,
        rb.UpdatedBy,
        rb.UpdatedOn,
        rb.DeletedBy,
        rb.DeletedOn
    from `Redblocks.Redblocks` rb
    where rb.ProjectId = p_ProjectId
      and rb.KeyNumber = p_KeyNumber;
end;

