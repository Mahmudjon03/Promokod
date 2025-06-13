namespace PromoRandom.Models;

public class User
{
    public long ChatId { get; set; }       // Телеграм-идентификатор пользователя
    public string Language { get; set; } = "tj"; // Язык пользователя, по умолчанию таджикский (tj)
    public string? Name { get; set; }      // Имя пользователя
    public string? Phone { get; set; }     // Телефон
}