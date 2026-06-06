using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentScoreManagement.Application.Common.ApiResponse;
using StudentScoreManagement.Application.DTOs.Auth;
using StudentScoreManagement.Application.Interfaces;

namespace StudentScoreManagement.Api.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(dto, cancellationToken);
        if (result is null)
        {
            return Unauthorized(ApiResponse<object?>.Fail("Login failed", new[] { "Invalid username or password." }));
        }

        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Login successful"));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result, "Register successful"));
    }
}
