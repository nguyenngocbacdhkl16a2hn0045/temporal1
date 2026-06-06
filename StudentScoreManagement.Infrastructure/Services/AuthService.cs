using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StudentScoreManagement.Application.DTOs.Auth;
using StudentScoreManagement.Application.DTOs.Users;
using StudentScoreManagement.Application.Interfaces;
using StudentScoreManagement.Domain.Entities;
using StudentScoreManagement.Domain.Enums;
using StudentScoreManagement.Infrastructure.Data;

namespace StudentScoreManagement.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext dbContext,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var username = dto.Username.Trim();
        var user = await _dbContext.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Username == username, cancellationToken);

        if (user is null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed for username {Username}", username);
            return null;
        }

        _logger.LogInformation("Login successful for username {Username}", username);

        return new LoginResponseDto
        {
            AccessToken = CreateAccessToken(user),
            Username = user.Username,
            Role = user.Role.ToString(),
            FullName = user.FullName
        };
    }

    public async Task<AppUserResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default)
    {
        var username = dto.Username.Trim();
        if (await _dbContext.AppUsers.AnyAsync(x => x.Username == username, cancellationToken))
        {
            throw new InvalidOperationException("Username already exists.");
        }

        if (!Enum.TryParse<UserRole>(dto.Role, true, out var role))
        {
            throw new InvalidOperationException("Invalid role.");
        }

        if (role == UserRole.Student && dto.StudentId.HasValue)
        {
            var studentExists = await _dbContext.Students.AnyAsync(x => x.Id == dto.StudentId.Value, cancellationToken);
            if (!studentExists)
            {
                throw new InvalidOperationException("StudentId does not exist.");
            }
        }

        var user = new AppUser
        {
            Username = username,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim(),
            Role = role,
            StudentId = dto.StudentId,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.AppUsers.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created user {Username} with role {Role}", user.Username, user.Role);
        return user.ToResponseDto();
    }

    private string CreateAccessToken(AppUser user)
    {
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expireMinutes = int.TryParse(_configuration["Jwt:ExpireMinutes"], out var minutes) ? minutes : 60;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (user.StudentId.HasValue)
        {
            claims.Add(new Claim("studentId", user.StudentId.Value.ToString()));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
