using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StudentScoreManagement.Application.Interfaces;
using StudentScoreManagement.Domain.Entities;
using StudentScoreManagement.Domain.Enums;

namespace StudentScoreManagement.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (dbContext.Database.IsSqlite())
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
        else
        {
            await dbContext.Database.MigrateAsync();
        }

        if (await dbContext.AppUsers.AnyAsync())
        {
            return;
        }

        var admin = new AppUser
        {
            Username = "admin",
            PasswordHash = passwordHasher.Hash("Admin@123"),
            FullName = "System Administrator",
            Email = "admin@student-score.local",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.AppUsers.AddAsync(admin);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Seeded default admin user: {Username}", admin.Username);
    }
}
