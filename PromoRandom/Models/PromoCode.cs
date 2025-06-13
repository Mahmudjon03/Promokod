namespace PromoRandom.Models;

public class PromoCode
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public long UserChatId { get; set; }
    public int OrderId { get; set; }
    public int GiveId { get; set; }
    public string Status { get; set; } = "active"; // Статус может быть 'active', 'won', 'expired'
    public DateTime SubmittedAt { get; set; }
}
