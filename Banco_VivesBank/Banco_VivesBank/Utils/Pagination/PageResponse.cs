namespace Banco_VivesBank.Utils.Pagination;

public class PageResponse<T>
{
    public List<T> Content { get; set; }
    public int TotalPages { get; set; }
    public long TotalElements { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    public bool Empty { get; set; }
    public bool First { get; set; }
    public bool Last { get; set; }
    public string SortBy { get; set; }
    public string Direction { get; set; }

    public static PageResponse<T> FromPage(Page<T> page, string sortBy, string direction)
    {
        return new PageResponse<T>
        {
            Content = page.Content,
            TotalPages = page.TotalPages,
            TotalElements = page.TotalElements,
            PageSize = page.PageSize,
            PageNumber = page.PageNumber,
            Empty = !page.HasContent,
            First = page.IsFirst,
            Last = page.IsLast,
            SortBy = sortBy,
            Direction = direction
        };
    }
}