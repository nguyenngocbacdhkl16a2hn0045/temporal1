using System.ComponentModel.DataAnnotations;

namespace StudentScoreManagement.Application.DTOs.Students;

public class StudentCreateDto
{
    [Required]
    public string StudentCode { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }
    public string Gender { get; set; } = "Unknown";
    public string ClassName { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
}
