USE DynamicFormBuilderDB;
GO

ALTER TABLE [user]
ADD CONSTRAINT fk_user_key
FOREIGN KEY (role_id) REFERENCES [role](role_id)
ON DELETE SET DEFAULT;

ALTER TABLE form 
ADD CONSTRAINT fk_form_group_id 
FOREIGN KEY (form_group_id) REFERENCES form_group(form_group_id)
ON DELETE CASCADE;

ALTER TABLE menu
ADD CONSTRAINT fk_parent_menu_id
FOREIGN KEY (parent_menu_id) REFERENCES menu(menu_id)
ON DELETE NO ACTION;

ALTER TABLE [authorization]
ADD CONSTRAINT fk_menu_id_auth
FOREIGN KEY (menu_id) REFERENCES menu(menu_id)
ON DELETE CASCADE;

ALTER TABLE [authorization]
ADD CONSTRAINT fk_role_id_auth 
FOREIGN KEY (role_id) REFERENCES [role](role_id)
ON DELETE CASCADE;