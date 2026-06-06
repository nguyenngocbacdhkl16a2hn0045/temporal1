using System.ComponentModel.DataAnnotations;

namespace StudentScoreManagement.Application.DTOs.Auth;

public class RegisterRequestDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Student";

    public int? StudentId { get; set; }
}
