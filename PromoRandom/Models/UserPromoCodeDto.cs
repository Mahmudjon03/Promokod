namespace PromoRandom.Models;

public class UserPromoCodeDto
{
    public string Code { get; set; } = "";
    public DateTime IssuedAt { get; set; }
    public double OrderPrice { get; set; }
}