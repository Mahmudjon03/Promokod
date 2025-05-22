namespace PromoRandom.Models;

public class User
{
    public int Id { get; set; }
    public string? Language { get; set; }  // Язык, может быть null, если не выбран
    public string? Name { get; set; }      // Имя пользователя
    public string? Phone { get; set; }     // Телефон
    public long ChatId { get; set; }       // Телеграм-идентификатор пользователя
}