

using DynamicFormBuilder.API.Constants;
using DynamicFormBuilder.API.Filters;
using DynamicFormBuilder.API.DTOs.Authorization;
using DynamicFormBuilder.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DynamicFormBuilder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        
        public AuthorizationController(IAuthorizationService authorizationService)
        {
            _authorizationService=authorizationService;
        }

        [HttpGet("authorizations/{id}")]
        [RequirePermission("/admin/authorizations",PermissionType.CanView)]
        public async Task<IActionResult> GetAuthorizationsByRoleId(byte id)
        {
            IEnumerable<AuthorizationResponseDto> authorizations = await _authorizationService.GetAuthorizationsByRoleIdAsync(id);
            return Ok(authorizations);
        }
        [HttpPatch("{id}")]
        [RequirePermission("/admin/authorizations",PermissionType.CanEdit)]
        public async Task<IActionResult> UpdateAuthorizations(byte id,List<AuthorizationUpdateDto> dtos)
        {
            IEnumerable<AuthorizationResponseDto> authorizations = await _authorizationService.UpdateAuthorizationsAsync(id,dtos);
            return Ok(authorizations);
        }
    }
}