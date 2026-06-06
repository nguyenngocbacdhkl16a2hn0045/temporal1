namespace StudentScoreManagement.Domain.Entities;

public class Subject
{
    public int Id { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int Credit { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Score> Scores { get; set; } = new List<Score>();
}
