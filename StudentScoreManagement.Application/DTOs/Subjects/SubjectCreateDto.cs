using System.ComponentModel.DataAnnotations;

namespace StudentScoreManagement.Application.DTOs.Subjects;

public class SubjectCreateDto
{
    [Required]
    public string SubjectCode { get; set; } = string.Empty;

    [Required]
    public string SubjectName { get; set; } = string.Empty;

    [Range(1, 20)]
    public int Credit { get; set; } = 3;
}
