using StudentScoreManagement.Application.Common.Pagination;

namespace StudentScoreManagement.Application.Common.Filtering;

public class StudentQueryParameters : PaginationQuery
{
    public string? Keyword { get; set; }
    public string? ClassName { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}
