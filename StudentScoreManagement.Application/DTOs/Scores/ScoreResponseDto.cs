namespace StudentScoreManagement.Application.DTOs.Scores;

public class ScoreResponseDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public decimal ScoreValue { get; set; }
    public string Semester { get; set; } = string.Empty;
    public string SchoolYear { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
