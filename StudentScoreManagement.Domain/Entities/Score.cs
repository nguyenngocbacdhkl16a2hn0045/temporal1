namespace StudentScoreManagement.Domain.Entities;

public class Score
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int SubjectId { get; set; }
    public decimal ScoreValue { get; set; }
    public string Semester { get; set; } = string.Empty;
    public string SchoolYear { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Student Student { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
}
