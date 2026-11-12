-- DependsOn: ScriptHistory, Redblocks
drop procedure if exists `Redblocks.usp_SelectRedblockById`;
create procedure if not exists `Redblocks.usp_SelectRedblockById`(
    p_RedblockId bigint)
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
    where rb.RedblockId = p_RedblockId;
end;

