namespace StudentScoreManagement.Application.DTOs.Users;

public class AppUserResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? StudentId { get; set; }
    public DateTime CreatedAt { get; set; }
}
