using StudentScoreManagement.Application.Common.Pagination;

namespace StudentScoreManagement.Application.Common.Filtering;

public class ScoreQueryParameters : PaginationQuery
{
    public string? StudentCode { get; set; }
    public string? StudentName { get; set; }
    public string? ClassName { get; set; }
    public int? SubjectId { get; set; }
    public string? Semester { get; set; }
    public string? SchoolYear { get; set; }
    public decimal? MinScore { get; set; }
    public decimal? MaxScore { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}
