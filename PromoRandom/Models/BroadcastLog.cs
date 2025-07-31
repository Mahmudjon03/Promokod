namespace PromoRandom.Models;

public class BroadcastLog
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public int SuccessCount { get; set; }
    public int TotalCount { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public string Language { get; set; } // "all", "ru", "tj", "uz", "en"
}
