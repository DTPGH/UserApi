namespace UserApi.DTOs.Responses;
public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Content { get; set; }
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
}