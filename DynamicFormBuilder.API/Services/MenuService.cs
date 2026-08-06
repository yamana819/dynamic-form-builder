using DynamicFormBuilder.API.DTOs.Menu;
using DynamicFormBuilder.API.Data;
using DynamicFormBuilder.API.Models;
using Microsoft.EntityFrameworkCore;
using DynamicFormBuilder.API.Exceptions;

namespace DynamicFormBuilder.API.Services;


public class MenuService : IMenuService
{
    private readonly DynamicFormBuilderDbContext _context;

    private readonly IAuthorizationService _authorizationService;

    public MenuService(DynamicFormBuilderDbContext context,IAuthorizationService authorizationService)
    {
        _context=context;
        _authorizationService=authorizationService;
    }
    public async Task<IEnumerable<MenuResponseDto>> GetMenusByRoleIdAsync(byte roleId)
    {
        var menus = await _context.Menus
                        .Where(m=>!m.IsDeleted)
                        .Include(m=>m.Authorizations.Where(a=>a.RoleId==roleId))
                        .OrderBy(m=>m.MenuId)
                        .AsNoTracking()
                        .ToListAsync();
        var menuDictionary = new Dictionary<int,MenuResponseDto>();
        foreach (var menu in menus)
        {
            var auth = menu.Authorizations
                        .Where(a=>a.CanView==true)
                        .FirstOrDefault();
            if (auth == null)
            {
                continue;
            }
            menuDictionary.Add(menu.MenuId,new MenuResponseDto
            {
                MenuId=menu.MenuId,
                ParentMenuId=menu.ParentMenuId,
                MenuName=menu.MenuName,
                Href=menu.Href,
                SubMenus= new List<MenuResponseDto>(),
                CanCreate=auth.CanCreate,
                CanEdit=auth.CanEdit,
                CanDelete=auth.CanDelete
            });
        }
        var rootMenus = new List<MenuResponseDto>();
        foreach (var menuDto in menuDictionary.Values)
        {
            if (menuDto.ParentMenuId.HasValue && menuDictionary.ContainsKey(menuDto.ParentMenuId.Value))
            {
                var parent =menuDictionary[menuDto.ParentMenuId.Value];
                parent.SubMenus.Add(menuDto);
            }
            else
            {
                rootMenus.Add(menuDto);
            }
        }
        return rootMenus;
    }
    public async Task CreateMenuForFormGroupAsync(string formGroupCode,string formGroupName,byte creatorRoleId)
    {
        Menu menu = new Menu
        {
            ParentMenuId=null,
            MenuName=formGroupName,
            Href=$"/forms/{formGroupCode}",
        };
        _context.Add(menu);
        await _context.SaveChangesAsync();
        await _authorizationService.CreateAuthorizationsForNewMenuAsync(menu.MenuId,creatorRoleId);
    }
    public async Task CreateMenuForFormAsync(Guid formId,string formGroupCode,string formName,byte creatorRoleId)
    {
        Menu? parentMenu = await _context.Menus
                    .Where(m=>m.Href==$"/forms/{formGroupCode}" && !m.IsDeleted)
                    .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Menü oluştururken ilgili menü grubu bulunamadı.");
        Menu menu = new Menu
        {
            ParentMenuId=parentMenu.MenuId,
            MenuName=formName,
            Href=$"/forms/{formGroupCode}/{formId}"
        };
        _context.Menus.Add(menu);
        await _context.SaveChangesAsync();
        await _authorizationService.CreateAuthorizationsForNewMenuAsync(menu.MenuId,creatorRoleId);
    }

    public async Task UpdateMenuForFormGroupAsync(string formGroupCode,string newName)
    {
        Menu? menu =await _context.Menus
                    .Where(m=>m.Href==$"/forms/{formGroupCode}" && !m.IsDeleted)
                    .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Menü güncellenirken ilgili menü grubu bulunamadı.");
        menu.MenuName=newName;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateMenuForFormAsync(Guid formId,string formGroupCode,string newName)
    {
        Menu? menu = await _context.Menus
                        .Where(m=>m.Href==$"/forms/{formGroupCode}/{formId}" && !m.IsDeleted)
                        .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Menü güncellenirken ilgili menü bulunamadı.");
        menu.MenuName=newName;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteMenuForFormGroupAsync(string formGroupCode)
    {
        var menus = await _context.Menus
                        .Where(m=>m.Href.StartsWith($"/forms/{formGroupCode}") || m.Href.StartsWith($"/forms/{formGroupCode}/") && !m.IsDeleted)
                        .ToListAsync();
        if (!menus.Any())
        {
            throw new ResourceNotFoundException("Silme işlemi sırasında form grubuna ait menü bulunamadı.");
        }
        foreach (var menu in menus)
        {
            menu.IsDeleted=true;
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteMenuForFormAsync(Guid formId,string formGroupCode)
    {
        Menu? menu = await _context.Menus
                        .Where(m=>m.Href==$"/forms/{formGroupCode}/{formId}" && !m.IsDeleted)
                        .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Menü silinirken ilgili menü bulunamadı.");
        menu.IsDeleted=true;
        await _context.SaveChangesAsync();
    }
}