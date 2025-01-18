namespace Banco_VivesBank.Utils.Pagination;

public class Page<T>
{
    public List<T> Content { get; set; } 
    public int TotalPages { get; set; } 
    public long TotalElements { get; set; } 
    public int PageSize { get; set; } 
    public int PageNumber { get; set; } 
    public bool IsFirst { get; set; } 
    public bool IsLast { get; set; } 
    public bool HasContent => Content.Any(); 
}