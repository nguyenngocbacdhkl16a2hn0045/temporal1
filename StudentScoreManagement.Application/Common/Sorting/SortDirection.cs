namespace StudentScoreManagement.Application.Common.Sorting;

public static class SortDirection
{
    public const string Asc = "asc";
    public const string Desc = "desc";

    public static bool IsDescending(string? sortOrder)
    {
        return string.Equals(sortOrder, Desc, StringComparison.OrdinalIgnoreCase);
    }
}
