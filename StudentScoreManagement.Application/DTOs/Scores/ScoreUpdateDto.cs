using System.ComponentModel.DataAnnotations;

namespace StudentScoreManagement.Application.DTOs.Scores;

public class ScoreUpdateDto
{
    [Range(0, 10)]
    public decimal ScoreValue { get; set; }

    [Required]
    public string Semester { get; set; } = string.Empty;

    [Required]
    public string SchoolYear { get; set; } = string.Empty;
}
