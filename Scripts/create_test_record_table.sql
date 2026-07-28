USE DynamicFormBuilderDB;
GO

CREATE TABLE test_record (
    test_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    test_type TINYINT,
    test_serial_number NVARCHAR(100),
    test_size INT DEFAULT 5,
)