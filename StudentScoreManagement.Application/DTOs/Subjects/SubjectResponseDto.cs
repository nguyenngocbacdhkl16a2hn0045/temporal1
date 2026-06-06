namespace StudentScoreManagement.Application.DTOs.Subjects;

public class SubjectResponseDto
{
    public int Id { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int Credit { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
