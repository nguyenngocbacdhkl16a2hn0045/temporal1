using StudentScoreManagement.Domain.Enums;

namespace StudentScoreManagement.Domain.Entities;

public class AppUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public int? StudentId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Student? Student { get; set; }
}
