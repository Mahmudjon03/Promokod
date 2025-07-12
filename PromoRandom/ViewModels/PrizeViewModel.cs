namespace PromoRandom.ViewModels;

public class PrizeViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ImagePath { get; set; } = "";
    public string WinnerName { get; set; } = "";
    public string WinnerPromoCode { get; set; } = ""; // Новое свойство
    public int Priority { get; set; }
    public int SpinDuration { get; set; }
    public string Status { get; set; } = "waiting";
    public int Quantity { get; set; }
    public int WonCount { get; set; }
    public int RequiredPromoCodeCount { get; set; }
    public List<string> WinnerNames { get; set; } = [];
    public List<string> WinnerPromoCodes { get; set; } = []; // Новое свойство
}