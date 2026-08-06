using System;
using System.Collections.Generic;
using DynamicFormBuilder.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DynamicFormBuilder.API.Data;

public partial class DynamicFormBuilderDbContext : DbContext
{
    public DynamicFormBuilderDbContext(DbContextOptions<DynamicFormBuilderDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Authorization> Authorizations { get; set; }

    public virtual DbSet<Form> Forms { get; set; }

    public virtual DbSet<FormGroup> FormGroups { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Authorization>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.MenuId }).HasName("PK__authoriz__A2C36A618FB47067");

            entity.ToTable("authorization");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.MenuId).HasColumnName("menu_id");
            entity.Property(e => e.CanCreate).HasColumnName("can_create");
            entity.Property(e => e.CanDelete).HasColumnName("can_delete");
            entity.Property(e => e.CanEdit).HasColumnName("can_edit");
            entity.Property(e => e.CanView).HasColumnName("can_view");

            entity.HasOne(d => d.Menu).WithMany(p => p.Authorizations)
                .HasForeignKey(d => d.MenuId)
                .HasConstraintName("fk_menu_id_auth");

            entity.HasOne(d => d.Role).WithMany(p => p.Authorizations)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("fk_role_id_auth");
        });

        modelBuilder.Entity<Form>(entity =>
        {
            entity.HasKey(e => e.FormId).HasName("PK__form__190E16C9723B53EA");

            entity.ToTable("form");

            entity.Property(e => e.FormId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("form_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at")
                .ValueGeneratedOnAdd();
            entity.Property(e=>e.FormGroupCode)
                .HasColumnName("group_code")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.FormName)
                .HasMaxLength(150)
                .HasColumnName("form_name");
            entity.Property(e => e.FormSchema).HasColumnName("form_schema");
            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.LastUpdate)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("last_update");
            entity.Property(e => e.TargetPrimaryKey)
                .HasMaxLength(128)
                .HasColumnName("target_primary_key");
            entity.Property(e => e.TargetTableName)
                .HasMaxLength(128)
                .HasColumnName("target_table_name");
            entity.Property(e => e.ViewName)
                .HasMaxLength(128)
                .HasColumnName("view_name");
            entity.Property(e=> e.IsPublished)
                .HasDefaultValue(false)
                .HasColumnName("is_published");

            entity.HasOne(d => d.FormGroup).WithMany(p => p.Forms)
                .HasForeignKey(d => d.FormGroupCode)
                .HasConstraintName("fk_group_code");
        });

        modelBuilder.Entity<FormGroup>(entity =>
        {
            entity.HasKey(e => e.FormGroupCode).HasName("pk_form_group_code");

            entity.ToTable("form_group");

            entity.HasIndex(e => e.FormGroupName, "UQ__form_gro__71018BFE166D3F3B").IsUnique();

            entity.Property(e=>e.FormGroupCode)
                .HasColumnName("group_code")
                .HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.FormGroupName)
                .HasMaxLength(150)
                .HasColumnName("form_group_name");
            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.LastUpdate)
                .HasColumnType("datetime")
                .HasColumnName("last_update");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__menu__4CA0FADC1E5D1BC7");

            entity.ToTable("menu");

            entity.Property(e => e.MenuId).HasColumnName("menu_id");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.Href)
                .HasMaxLength(255)
                .HasColumnName("href");
            entity.Property(e => e.MenuName)
                .IsRequired()
                .HasMaxLength(155)
                .HasColumnName("menu_name");
            entity.Property(e => e.ParentMenuId)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("parent_menu_id");
            entity.Property(e=>e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");

            entity.HasOne(d => d.ParentMenu).WithMany(p => p.InverseParentMenu)
                .HasForeignKey(d => d.ParentMenuId)
                .HasConstraintName("fk_parent_menu_id");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__role__760965CC422F2AF5");

            entity.ToTable("role");

            entity.HasIndex(e => e.RoleName, "UQ__role__783254B1173ED476").IsUnique();

            entity.Property(e => e.RoleId)
                .ValueGeneratedOnAdd()
                .HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(150)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__user__B9BE370F5922B728");

            entity.ToTable("user");

            entity.HasIndex(e => e.UserName, "UQ__user__7C9273C490926903").IsUnique();

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("user_id");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.RoleId)
                .HasDefaultValue((byte)1)
                .HasColumnName("role_id");
            entity.Property(e => e.UserLastActiveDate)
                .HasColumnType("datetime")
                .HasColumnName("user_last_active_date");
            entity.Property(e => e.UserName)
                .HasMaxLength(150)
                .HasColumnName("user_name");
            entity.Property(e => e.UserStartDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("user_start_date")
                .ValueGeneratedOnAdd();

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_key");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}