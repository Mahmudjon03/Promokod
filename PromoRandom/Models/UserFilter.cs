namespace PromoRandom.Models;

public class UserFilter
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string SortBy { get; internal set; }
    public string SortOrder { get; internal set; }
}
