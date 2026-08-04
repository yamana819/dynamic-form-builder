using DynamicFormBuilder.API.Data;
using DynamicFormBuilder.API.DTOs.Role;
using DynamicFormBuilder.API.Exceptions;
using DynamicFormBuilder.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DynamicFormBuilder.API.Services;


public class RoleService : IRoleService
{
    private readonly DynamicFormBuilderDbContext _context;

    public RoleService(DynamicFormBuilderDbContext context)
    {
        _context=context;
    }

    private Role MapToRole(RoleCreateDto dto)
    {
        return new Role
        {
            RoleName=dto.RoleName
        };
    }
    
    private RoleResponseDto MapToDto(Role role)
    {
        return new RoleResponseDto
        {
            RoleId=role.RoleId,
            RoleName=role.RoleName
        };
    }

    private void UpdateEntityFromDto(Role role,RoleUpdateDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.RoleName) && (dto.RoleName != role.RoleName))
        {
            role.RoleName=dto.RoleName;
        }
    }

    public async Task<IEnumerable<RoleResponseDto>> GetAllRolesAsync(int pageNumber=1,int pageSize=50)
    {
        return await _context.Roles
                    .OrderBy(r=>r.RoleId)
                    .Skip((pageNumber-1)*pageSize)
                    .Take(pageSize)
                    .Select(r=> new RoleResponseDto
                    {
                        RoleId=r.RoleId,
                        RoleName=r.RoleName
                    })
                    .ToListAsync();
    }

    public async Task<RoleResponseDto> GetRoleAsync(byte roleId)
    {
        Role? role = await _context.Roles
                    .Where(r=>r.RoleId==roleId)
                    .AsNoTracking()
                    .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Role bulunamadı.");
        return MapToDto(role);
    }

    public async Task<RoleResponseDto> CreateRoleAsync(RoleCreateDto dto)
    {
        bool roleExists = await _context.Roles.AnyAsync(r=>r.RoleName==dto.RoleName);
        if (roleExists)
        {
            throw new ConflictException("Bu rol ismi zaten kullanılıyor.");   
        }
        Role role = MapToRole(dto);
        _context.Add(role);
        await _context.SaveChangesAsync();
        return MapToDto(role);
    }

    public async Task<RoleResponseDto> UpdateRoleAsync(byte roleId,RoleUpdateDto dto)
    {
        Role? role = await _context.Roles
                        .Where(r=>r.RoleId==roleId)
                        .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Güncelleme işlemi sırasında role bulunamadı");
        if (!string.IsNullOrWhiteSpace(dto.RoleName) && (role.RoleName != dto.RoleName))
        {
            bool roleExists = await _context.Roles.AnyAsync(r=>r.RoleName==dto.RoleName);
            if (roleExists)
            {
                throw new ConflictException("Bu rol ismi zaten kullanılıyor.");   
            }
        }
        UpdateEntityFromDto(role,dto);
        await _context.SaveChangesAsync();
        return MapToDto(role);
    }
    public async Task DeleteRoleAsync(byte roleId)
    {
        Role? role = await _context.Roles
                        .Where(r=>r.RoleId==roleId)
                        .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Silme işlemi sırasında rol bulunamadı");
        _context.Remove(role);
        await _context.SaveChangesAsync();
    }
}