namespace PromoRandom.ViewModels;

public class UserViewModel
{
    public long ChatId { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? PersonalPhone { get; set; }
    public string Language { get; set; } = "";
    public DateTime RegisteredAt { get; set; }

    public int PromoCodeCount { get; set; }
    public int OrderCount { get; set; }
    public double TotalOrderAmount { get; set; }
}