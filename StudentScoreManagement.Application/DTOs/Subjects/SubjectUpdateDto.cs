using System.ComponentModel.DataAnnotations;

namespace StudentScoreManagement.Application.DTOs.Subjects;

public class SubjectUpdateDto
{
    [Required]
    public string SubjectName { get; set; } = string.Empty;

    [Range(1, 20)]
    public int Credit { get; set; } = 3;
}
