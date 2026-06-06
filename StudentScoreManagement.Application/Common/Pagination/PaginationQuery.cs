namespace StudentScoreManagement.Application.Common.Pagination;

public class PaginationQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public void Normalize()
    {
        if (PageNumber <= 0)
        {
            PageNumber = 1;
        }

        if (PageSize <= 0)
        {
            PageSize = 10;
        }

        if (PageSize > 100)
        {
            PageSize = 100;
        }
    }
}
