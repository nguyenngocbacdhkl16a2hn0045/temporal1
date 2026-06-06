using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StudentScoreManagement.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Data Source=student-score.db";
        var provider = Environment.GetEnvironmentVariable("DatabaseProvider") ?? "Sqlite";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
        else
        {
            optionsBuilder.UseSqlite(connectionString);
        }

        return new AppDbContext(optionsBuilder.Options);
    }
}
