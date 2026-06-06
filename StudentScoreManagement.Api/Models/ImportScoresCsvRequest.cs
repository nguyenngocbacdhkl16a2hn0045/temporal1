using System.ComponentModel.DataAnnotations;

namespace StudentScoreManagement.Api.Models;

public class ImportScoresCsvRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;
}
