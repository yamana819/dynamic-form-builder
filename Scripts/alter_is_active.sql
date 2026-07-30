USE DynamicFormBuilderDB;
GO

DECLARE @ConstraintName nvarchar(200);
SELECT @ConstraintName = name 
FROM sys.default_constraints
WHERE parent_object_id = object_id('[user]') 
AND col_name(parent_object_id, parent_column_id) = 'is_active';

IF @ConstraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE [user] DROP CONSTRAINT ' + @ConstraintName);
END
GO

EXEC sp_rename '[user].is_active', 'is_deleted', 'COLUMN';
GO

ALTER TABLE [user] ADD CONSTRAINT DF_User_IsDeleted DEFAULT 0 FOR is_deleted;
GO