namespace PromoRandom.ViewModels;

public class UserPromoDetailsViewModel
{
    public string Code { get; set; } = "";              // Промокод
    public string Status { get; set; } = "";            // Статус (active, won и т.д.)
    public DateTime SubmittedAt { get; set; }           // Дата получения промокода

    public int OrderNumber { get; set; }                // Номер заказа
    public double OrderAmount { get; set; }             // Сумма заказа
    public DateTime OrderDate { get; set; }             // Дата заказа

    public string GiveName { get; set; } = "";          // Название розыгрыша
}
