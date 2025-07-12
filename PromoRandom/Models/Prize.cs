using System.ComponentModel.DataAnnotations.Schema;

namespace PromoRandom.Models;

public class Prize
{
    public int Id { get; set; }
    public int GiveId { get; set; }
    public string Name { get; set; } = "";
    public string ImagePath { get; set; } = "";
    public int Priority { get; set; }
    public int SpinDuration { get; set; }
    public int Quantity { get; set; }
    public int RequiredPromoCodeCount { get; set; }
    public int WinCount { get; set; }

    [NotMapped]
    public bool IsAvailable => WinCount < Quantity;

    // Добавлено свойство для количества победителей
    [NotMapped]
    public int WinnerCount { get; set; }

    // Вычисляемое свойство для остатка
    [NotMapped]
    public int Remaining => Quantity - WinnerCount;
}