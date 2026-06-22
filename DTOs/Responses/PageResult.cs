namespace UserApi.DTOs.Responses;

public class PageResult<T>
{
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
}