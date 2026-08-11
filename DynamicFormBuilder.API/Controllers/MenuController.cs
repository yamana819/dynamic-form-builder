using DynamicFormBuilder.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DynamicFormBuilder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : BaseApiController
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService, IUserService userService) : base(userService)
        {
            _menuService = menuService;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyMenus()
        {
            byte roleId = await GetCurrentUserRoleIdAsync(); 
            
            var menus = await _menuService.GetMenusByRoleIdAsync(roleId);
            return Ok(menus);
        }
    }
}