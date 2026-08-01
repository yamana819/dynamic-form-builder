USE DynamicFormBuilderDB;
GO

ALTER TABLE form 
ALTER COLUMN is_deleted BIT NOT NULL;

ALTER TABLE form_group
ALTER COLUMN is_deleted BIT NOT NULL;