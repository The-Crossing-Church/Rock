-- un-link any 'Team Group' Groups from all existing Campuses
UPDATE [Campus]
SET [TeamGroupId] = NULL;
GO

-- TODO(Jason H): What other Entities to I need to delete before deleting a Group??
--                Should I even be deleting Groups? Maybe they should simply be inactivated/archived instead...
--                Risky.

-- delete all Groups of GroupType 'TeamGroup'
DECLARE @GroupTypeId [int] = (SELECT [Id] FROM [GroupType] WHERE [Guid] = 'BADD7A6C-1FB3-4E11-A721-6D1377C6958C');

IF (@GroupTypeId IS NOT NULL)
BEGIN
    DELETE FROM [Group]
    WHERE ([GroupTypeId] = @GroupTypeId);

    -- OR --

    --UPDATE [Group]
    --SET [IsActive] = 0
    --    , [InactiveDateTime] = GETDATE()
    --WHERE ([GroupTypeId] = @GroupTypeId);

    -- OR --

    --UPDATE [Group]
    --SET [IsArchived] = 1
    --    , [ArchivedDateTime] = GETDATE()
    --WHERE ([GroupTypeId] = @GroupTypeId);
END
