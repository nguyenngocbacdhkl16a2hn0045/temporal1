using StudentScoreManagement.Application.DTOs.Users;

namespace StudentScoreManagement.Application.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<AppUserResponseDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<AppUserResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
