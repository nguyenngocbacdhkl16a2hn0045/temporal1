using System.ComponentModel.DataAnnotations;

namespace StudentScoreManagement.Application.DTOs.Scores;

public class ScoreCreateDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int SubjectId { get; set; }

    [Range(0, 10)]
    public decimal ScoreValue { get; set; }

    [Required]
    public string Semester { get; set; } = string.Empty;

    [Required]
    public string SchoolYear { get; set; } = string.Empty;
}
