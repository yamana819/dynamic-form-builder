



using DynamicFormBuilder.API.DTOs;
using DynamicFormBuilder.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DynamicFormBuilder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class AuthenticationController:ControllerBase
    {
        private IAuthenticationService _authenticationService;
        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService=authenticationService;
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            AuthenticationResponseDto authenticationResponse = await _authenticationService.LoginAsync(dto);
            return Ok(authenticationResponse);
        }
    }
}

