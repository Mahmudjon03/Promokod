namespace PromoRandom.Models;

public class Order
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public double Amount { get; set; }
    public DateTime Date { get; set; }
}
