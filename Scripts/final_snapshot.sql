-- =========================================================================
-- PROJECT: Dynamic Form Builder
-- DESCRIPTION: Final Consolidated Database Schema Snapshot
-- =========================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DynamicFormBuilderDB')
BEGIN
    CREATE DATABASE DynamicFormBuilderDB;
END
GO

USE DynamicFormBuilderDB;
GO

-- =========================================================================
-- 1. CORE TABLES (RBAC & User Management)
-- =========================================================================

-- TABLE: role
-- Stores user roles for the Role-Based Access Control (RBAC) system.
CREATE TABLE [role] (
    role_id TINYINT PRIMARY KEY IDENTITY(1,1),
    role_name NVARCHAR(150) UNIQUE NOT NULL
);
GO

-- TABLE: user
-- Stores user credentials, tracking details, and soft deletion flag.
CREATE TABLE [user] (
    user_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    user_name NVARCHAR(150) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL UNIQUE,
    role_id TINYINT NOT NULL DEFAULT 2,
    user_start_date DATETIME NOT NULL DEFAULT GETDATE(),
    password_hash NVARCHAR(255) NOT NULL,
    user_last_active_date DATETIME NULL,
    is_deleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT fk_user_role FOREIGN KEY (role_id) REFERENCES [role](role_id) ON DELETE SET DEFAULT
);
GO

-- TABLE: menu
-- Defines the dynamic sidebar navigation links.
CREATE TABLE menu (
    menu_id INT PRIMARY KEY IDENTITY(1,1),
    parent_menu_id INT NULL,
    menu_name NVARCHAR(155) NOT NULL,
    display_order INT,
    href NVARCHAR(255) NOT NULL,
    is_deleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT fk_parent_menu_id FOREIGN KEY (parent_menu_id) REFERENCES menu(menu_id) ON DELETE NO ACTION
);
GO
-- Non-clustered index for faster URL/route lookups
CREATE NONCLUSTERED INDEX IX_Menu_href ON menu(href);
GO

-- TABLE: authorization
-- Mapping table defining granular permissions per role and menu.
CREATE TABLE [authorization] (
    role_id TINYINT NOT NULL,
    menu_id INT NOT NULL,
    can_view BIT NOT NULL,
    can_create BIT NOT NULL,
    can_edit BIT NOT NULL,
    can_delete BIT NOT NULL,
    PRIMARY KEY(role_id, menu_id),
    CONSTRAINT fk_role_id_auth FOREIGN KEY (role_id) REFERENCES [role](role_id) ON DELETE CASCADE,
    CONSTRAINT fk_menu_id_auth FOREIGN KEY (menu_id) REFERENCES menu(menu_id) ON DELETE CASCADE
);
GO

-- =========================================================================
-- 2. DYNAMIC FORM MANAGEMENT TABLES
-- =========================================================================

-- TABLE: form_group
-- Groups dynamic forms under specific categories using a unique group_code.
CREATE TABLE form_group (
    group_code VARCHAR(50) NOT NULL PRIMARY KEY,
    form_group_name NVARCHAR(150) UNIQUE NOT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    last_update DATETIME NULL,
    is_deleted BIT NOT NULL DEFAULT 0
);
GO

-- TABLE: form
-- Stores the JSON schema, target mapping details, and publish status.
CREATE TABLE form (
    form_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    form_name NVARCHAR(150) NOT NULL,
    group_code VARCHAR(50) NOT NULL,
    target_table_name NVARCHAR(128),
    target_primary_key NVARCHAR(128),
    view_name NVARCHAR(128),
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    last_update DATETIME NULL,
    is_deleted BIT NOT NULL DEFAULT 0,
    is_published BIT NOT NULL DEFAULT 0,
    form_schema NVARCHAR(MAX) NOT NULL,
    CONSTRAINT fk_group_code FOREIGN KEY (group_code) REFERENCES form_group(group_code) ON DELETE NO ACTION ON UPDATE CASCADE
);
GO

-- =========================================================================
-- 3. TEST ENVIRONMENT & VIEWS
-- =========================================================================

-- TABLE: kayit_test
-- A sample target table used for testing dynamic insert/update operations.
CREATE TABLE kayit_test (
    kayit_kodu NVARCHAR(15) PRIMARY KEY,
    isim NVARCHAR(20) NOT NULL,
    soyisim NVARCHAR(25) NOT NULL,
    dogum_tarihi DATETIME NOT NULL,
    adres NVARCHAR(70),
    is_deleted BIT NOT NULL DEFAULT 0
);
GO

-- VIEW: vw_kayit_test
-- Filters out soft-deleted records for frontend data grid binding.
CREATE VIEW vw_kayit_test AS
SELECT 
    kayit_kodu,                  
    isim AS 'İsim',
    soyisim AS 'Soyisim',
    dogum_tarihi AS 'Doğum Tarihi',
    adres AS 'Adres'
FROM kayit_test
WHERE is_deleted = 0;
GO

-- =========================================================================
-- 4. SEED DATA (Initial Configuration)
-- =========================================================================

-- Insert Roles
INSERT INTO [role] (role_name) VALUES ('Admin'), ('User');
GO

-- Insert Default Admin Menus (Using final updated names)
INSERT INTO [menu] (menu_name, href) 
VALUES 
    ('Yetki Kontrol Paneli', '/admin/authorizations'),
    ('Kullanici Yönetim Paneli', '/admin/users'),
    ('Form Grupları', '/forms');
GO

-- Insert Authorizations
-- 1) Admin gets full access to all menus
INSERT INTO [authorization] (menu_id, role_id, can_create, can_delete, can_edit, can_view)
VALUES 
    (1, 1, 1, 1, 1, 1),
    (2, 1, 1, 1, 1, 1),
    (3, 1, 1, 1, 1, 1);
GO

-- 2) Normal User gets full access only to 'Form Grupları' (Menu ID: 3)
INSERT INTO [authorization] (menu_id, role_id, can_create, can_delete, can_edit, can_view)
VALUES 
    (3, 2, 1, 1, 1, 1);
GO