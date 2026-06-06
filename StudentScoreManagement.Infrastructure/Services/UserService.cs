using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentScoreManagement.Application.DTOs.Users;
using StudentScoreManagement.Application.Interfaces;
using StudentScoreManagement.Infrastructure.Data;

namespace StudentScoreManagement.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext dbContext, ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AppUserResponseDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _dbContext.AppUsers
            .AsNoTracking()
            .OrderBy(x => x.Username)
            .ToListAsync(cancellationToken);

        return users.Select(x => x.ToResponseDto()).ToList();
    }

    public async Task<AppUserResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return user?.ToResponseDto();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.AppUsers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        _dbContext.AppUsers.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted user {Username}", user.Username);

        return true;
    }
}
