namespace StudentScoreManagement.Application.DTOs.Import;

public class ImportResultDto
{
    public int TotalRows { get; set; }
    public int SuccessRows { get; set; }
    public int FailedRows { get; set; }
    public List<ImportErrorDto> Errors { get; set; } = new();
}

public class ImportErrorDto
{
    public int RowNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}
