using StudentScoreManagement.Application.DTOs.Auth;
using StudentScoreManagement.Application.DTOs.Users;

namespace StudentScoreManagement.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
    Task<AppUserResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default);
}
