USE DynamicFormBuilderDB;
GO

ALTER TABLE form 
ADD is_published BIT NOT NULL DEFAULT 0;