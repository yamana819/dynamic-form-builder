using DynamicFormBuilder.API.DTOs.Authorization;




namespace DynamicFormBuilder.API.Services;

public interface IAuthorizationService
{
    
    Task<IEnumerable<AuthorizationResponseDto>> GetAuthorizationsByRoleIdAsync(byte roleId);
    
    Task<IEnumerable<AuthorizationResponseDto>> UpdateAuthorizationsAsync(byte roleId,IEnumerable<AuthorizationUpdateDto> dtos);

    Task CreateAuthorizationsForNewRoleAsync(byte roleId);

    Task CreateAuthorizationsForNewMenuAsync(int menuId,byte creatorRoleId);
}