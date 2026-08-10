using DynamicFormBuilder.API.Constants;
using DynamicFormBuilder.API.Models;
using DynamicFormBuilder.API.Data;
using DynamicFormBuilder.API.DTOs.Authorization;
using Microsoft.EntityFrameworkCore;
using DynamicFormBuilder.API.Exceptions;

namespace DynamicFormBuilder.API.Services;


public class AuthorizationService : IAuthorizationService
{
    private readonly DynamicFormBuilderDbContext _context;
    
    public AuthorizationService(DynamicFormBuilderDbContext context)
    {
        _context=context;
    }

    private AuthorizationResponseDto MapToDto(Authorization authorization)
    {
        return new AuthorizationResponseDto
        {
            MenuName=authorization.Menu.MenuName,
            MenuId=authorization.MenuId,
            CanView=authorization.CanView,
            CanCreate=authorization.CanCreate,
            CanEdit=authorization.CanEdit,
            CanDelete=authorization.CanDelete
        };
    }

    public async Task<IEnumerable<AuthorizationResponseDto>> GetAuthorizationsByRoleIdAsync(byte roleId)
    {
        return await _context.Authorizations
                .Where(a=>a.RoleId==roleId)
                .Select(a=> new AuthorizationResponseDto
                {
                    MenuId = a.MenuId,
                    MenuName = a.Menu.MenuName, 
                    CanView = a.CanView,
                    CanCreate = a.CanCreate,
                    CanEdit = a.CanEdit,
                    CanDelete = a.CanDelete
                })
                .AsNoTracking()
                .ToListAsync();
    }

    public async Task<IEnumerable<AuthorizationResponseDto>> UpdateAuthorizationsAsync(byte roleId,IEnumerable<AuthorizationUpdateDto> dtos)
    {
        var authorizations = await _context.Authorizations
                                .Where(a=>a.RoleId==roleId)
                                .Include(a=>a.Menu)
                                .ToListAsync();
        bool isDuplicated = dtos.GroupBy(d=>d.MenuId).Any(d=>d.Count()>1);
        if (isDuplicated)
        {
            throw new BadRequestException("Bir yetkilendirme tekrarı var.");
        }
        var dtoDictionary = dtos.ToDictionary(d=>d.MenuId);
        foreach (var authorization in authorizations)
        {
            if (dtoDictionary.TryGetValue(authorization.MenuId,out var dto))
            {
                authorization.CanView=dto.CanView ?? authorization.CanView;
                authorization.CanCreate=dto.CanCreate ?? authorization.CanCreate;
                authorization.CanEdit=dto.CanEdit ?? authorization.CanEdit;
                authorization.CanDelete=dto.CanDelete ?? authorization.CanDelete;
            }
        }
        await _context.SaveChangesAsync();
        return authorizations.Select(MapToDto);
    }

    public async Task CreateAuthorizationsForNewRoleAsync(byte roleId)
    {
        var menuIds = await _context.Menus
                        .Select(m=>m.MenuId)
                        .ToListAsync();
        foreach (int menuId in menuIds)
        {
            _context.Authorizations.Add(new Authorization 
            {
                MenuId=menuId,
                RoleId=roleId,
                CanView=false,
                CanCreate=false,
                CanEdit=false,
                CanDelete=false
            });
        }
        await _context.SaveChangesAsync();
    }

    public async Task CreateAuthorizationsForNewMenuAsync(int menuId,byte creatorRoleId)
    {
        var roleIds = await _context.Roles
                        .Select(r=>r.RoleId)
                        .ToListAsync();
        foreach (byte roleId in roleIds)
        {
            if (roleId != DefaultValues.DefaultAdminRoleId && roleId!=creatorRoleId)
            {
                _context.Authorizations.Add(new Authorization
                {
                    MenuId=menuId,
                    RoleId=roleId,
                    CanView=false,
                    CanCreate=false,
                    CanEdit=false,
                    CanDelete=false
                });
            }
            else
            {
                _context.Authorizations.Add(new Authorization
                {
                    MenuId=menuId,
                    RoleId=roleId,
                    CanView=true,
                    CanCreate=true,
                    CanEdit=true,
                    CanDelete=true
                });
            }
        }
        await _context.SaveChangesAsync();
    }
}