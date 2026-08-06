USE DynamicFormBuilderDb;
GO

SELECT name FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID('[form_group]') AND col_name(parent_object_id, parent_column_id) = 'form_group_id';--Default constraint adını bulmak için script.
GO

SELECT name 
FROM sys.key_constraints
WHERE type='PK' AND parent_object_id = OBJECT_ID('form_group') ;--Primary key constraint adını bulmak için script.
GO

ALTER TABLE form 
DROP CONSTRAINT fk_form_group_id;
GO

ALTER TABLE form_group
DROP CONSTRAINT PK__form_gro__8ACAD5B432A45A1A;
GO

ALTER TABLE form_group
DROP CONSTRAINT DF__form_grou__form___2C3393D0;
GO

ALTER TABLE form_group  
DROP COLUMN form_group_id;
GO

ALTER TABLE form 
DROP COLUMN form_group_id;
GO

ALTER TABLE form_group
ADD group_code VARCHAR(50) NOT NULL;
GO

ALTER TABLE form_group
ADD CONSTRAINT pk_form_group_code PRIMARY KEY (group_code);
GO

ALTER TABLE form 
ADD group_code VARCHAR(50) NOT NULL;
GO

ALTER TABLE form 
ADD CONSTRAINT fk_group_code
FOREIGN KEY (group_code) REFERENCES form_group(group_code)
ON DELETE NO ACTION 
ON UPDATE CASCADE;
GO