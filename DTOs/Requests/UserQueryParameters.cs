using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs.Requests;

public class UserQueryParameters
{
    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than 0")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool Desc { get; set; }
}