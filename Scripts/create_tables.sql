IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DynamicFormBuilderDB')
BEGIN
    CREATE DATABASE DynamicFormBuilderDB;
END
GO

USE DynamicFormBuilderDB;
GO

CREATE TABLE [user] (
    user_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    user_name NVARCHAR(50) UNIQUE NOT NULL,
    role_id TINYINT NOT NULL DEFAULT 1,
    user_start_date DATETIME DEFAULT GETDATE(),
    password_hash NVARCHAR(255) NOT NULL,
    user_last_active_date DATETIME NULL,
    is_active BIT DEFAULT 1
);

CREATE TABLE form_group (
    form_group_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    form_group_name NVARCHAR(50) UNIQUE NOT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    last_update DATETIME NULL,
    is_deleted BIT DEFAULT 0
);

CREATE TABLE form (
    form_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    form_name NVARCHAR(50) NOT NULL,
    form_group_id UNIQUEIDENTIFIER NOT NULL,
    target_table_name NVARCHAR(50),
    target_primary_key NVARCHAR(50),
    view_name NVARCHAR(50),
    created_at DATETIME DEFAULT GETDATE(),
    last_update DATETIME DEFAULT NULL,
    is_deleted BIT DEFAULT 0,
    form_schema NVARCHAR(MAX) NOT NULL
);

CREATE TABLE [role] (
    role_id TINYINT PRIMARY KEY IDENTITY(1,1),
    role_name NVARCHAR(100) UNIQUE NOT NULL
);

CREATE TABLE [authorization] (
    role_id TINYINT NOT NULL,
    menu_id INT NOT NULL,
    can_view BIT NOT NULL,
    can_create BIT NOT NULL,
    can_edit BIT NOT NULL,
    can_delete BIT NOT NULL,
    PRIMARY KEY(role_id,menu_id)
);

CREATE TABLE menu (
    menu_id INT PRIMARY KEY IDENTITY(1,1),
    parent_menu_id INT DEFAULT NULL,
    menu_name NVARCHAR(50),
    display_order INT,
    href NVARCHAR(255)
);