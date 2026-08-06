USE DynamicFormBuilderDB;
GO


ALTER TABLE [menu]
ADD is_deleted BIT NOT NULL;
GO

ALTER TABLE [menu]
ADD CONSTRAINT DF_menu_is_deleted DEFAULT 0 FOR is_deleted;
GO