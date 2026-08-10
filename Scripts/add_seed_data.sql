USE DynamicFormBuilderDb;
GO

INSERT INTO [role] (role_name)
VALUES ('Admin');
GO

INSERT INTO [menu] (menu_name,href)
VALUES ('Yetki Kontrolü Ekranı','/admin/authorizations'),
        ('Admin Kullanıcı Paneli','/admin/users');
GO

INSERT INTO [authorization] (menu_id,role_id,can_create,can_delete,can_edit,can_view)
VALUES (1,1,1,1,1,1),
        (2,1,1,1,1,1);
GO

INSERT INTO [role] (role_name)
VALUES ('User');
GO

UPDATE [user]
SET role_id=1
WHERE ([user_id]='c91d433e-f36b-1410-8cfb-00617d72ed57');
GO

INSERT INTO [menu] (menu_name,href)
VALUES ('Form Grupları','/forms');
GO

INSERT INTO [authorization] (menu_id,role_id,can_create,can_delete,can_edit,can_view)
VALUES (3,1,1,1,1,1)
GO

INSERT INTO [menu] (menu_name,href)
VALUES ('Rol Yönetim Paneli','/admin/roles');
GO

INSERT INTO [authorization] (menu_id,role_id,can_create,can_delete,can_edit,can_view)
VALUES (6,1,1,1,1,1);
GO

INSERT INTO [authorization] (menu_id,role_id,can_create,can_delete,can_edit,can_view)
VALUES (3,2,1,1,1,1);
GO