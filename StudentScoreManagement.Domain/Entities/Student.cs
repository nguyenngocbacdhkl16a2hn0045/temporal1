using StudentScoreManagement.Domain.Enums;

namespace StudentScoreManagement.Domain.Entities;

public class Student
{
    public int Id { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Gender Gender { get; set; } = Gender.Unknown;
    public string ClassName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Score> Scores { get; set; } = new List<Score>();
    public ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();
}
