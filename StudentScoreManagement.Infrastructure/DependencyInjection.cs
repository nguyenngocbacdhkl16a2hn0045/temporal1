using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudentScoreManagement.Application.Interfaces;
using StudentScoreManagement.Infrastructure.Data;
using StudentScoreManagement.Infrastructure.Repositories;
using StudentScoreManagement.Infrastructure.Services;

namespace StudentScoreManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["DatabaseProvider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing.");

        services.AddDbContext<AppDbContext>(options =>
        {
            if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString);
                return;
            }

            options.UseSqlite(connectionString);
        });

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IScoreService, ScoreService>();
        services.AddScoped<IImportService, CsvImportService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        return services;
    }
}
