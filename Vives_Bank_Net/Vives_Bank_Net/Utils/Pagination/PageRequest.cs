namespace Vives_Bank_Net.Utils.Pagination;

public class PageRequest
{
    public int PageNumber { get; set; } = 0;
  
    public int PageSize { get; set; } = 10;

    public string SortBy { get; set; } = "Id";
    
    public string Direction { get; set; } = "ASC";
}
