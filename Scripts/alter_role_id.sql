USE DynamicFormBuilderDB;
GO



SELECT name FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID('[user]') AND col_name(parent_object_id, parent_column_id) = 'role_id';--Constraint adını bulmak için script.
GO

ALTER TABLE [user] DROP CONSTRAINT fk_user_key;
GO

ALTER TABLE [user]
DROP CONSTRAINT [DF__user__role_id__267ABA7A];
GO

ALTER TABLE [user]
ADD CONSTRAINT Df_user_role_id DEFAULT 2 FOR role_id;
GO

ALTER TABLE [user]
ADD CONSTRAINT fk_user_key
FOREIGN KEY (role_id) REFERENCES [role](role_id)
ON DELETE SET DEFAULT;
GO