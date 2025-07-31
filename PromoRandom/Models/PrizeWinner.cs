namespace PromoRandom.Models;

public class PrizeWinner
{
    public string PrizeName { get; set; } = "-";
    public string WinnerName { get; set; } = "-";
    public string PromoCode { get; set; } = "-"; // если надо
    public string ImagePath { get; set; } = "";  // ✅ добавлено
}

