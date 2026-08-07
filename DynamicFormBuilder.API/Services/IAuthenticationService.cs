


using DynamicFormBuilder.API.DTOs;

namespace DynamicFormBuilder.API.Services;


public interface IAuthenticationService
{
    Task<AuthenticationResponseDto> LoginAsync(LoginDto dto);
}