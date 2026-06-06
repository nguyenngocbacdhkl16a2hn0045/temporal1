using Microsoft.EntityFrameworkCore;
using StudentScoreManagement.Domain.Entities;

namespace StudentScoreManagement.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        var currentTimestampSql = Database.IsSqlite() ? "CURRENT_TIMESTAMP" : "GETUTCDATE()";

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StudentCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Gender).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.ClassName).HasMaxLength(100);
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(currentTimestampSql);
            entity.HasIndex(x => x.StudentCode).IsUnique();
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SubjectCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.SubjectName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Credit).HasDefaultValue(3);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(currentTimestampSql);
            entity.HasIndex(x => x.SubjectCode).IsUnique();
        });

        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasKey(x => x.Id);
            if (Database.IsSqlite())
            {
                entity.Property(x => x.ScoreValue).HasConversion<double>();
            }
            else
            {
                entity.Property(x => x.ScoreValue).HasColumnType("decimal(5,2)");
            }
            entity.Property(x => x.Semester).HasMaxLength(50).IsRequired();
            entity.Property(x => x.SchoolYear).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(currentTimestampSql);

            entity.HasOne(x => x.Student)
                .WithMany(x => x.Scores)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Subject)
                .WithMany(x => x.Scores)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.StudentId, x.SubjectId, x.Semester, x.SchoolYear })
                .IsUnique();
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(currentTimestampSql);
            entity.HasIndex(x => x.Username).IsUnique();

            entity.HasOne(x => x.Student)
                .WithMany(x => x.AppUsers)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
