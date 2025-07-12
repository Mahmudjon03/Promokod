namespace PromoRandom.Models;

using System.ComponentModel.DataAnnotations;

public class Article
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Заголовок обязателен")]
    [StringLength(200, ErrorMessage = "Заголовок не должен превышать 200 символов")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Содержание обязательно")]
    public string Content { get; set; } = string.Empty;

    public string? ImagePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public int Views { get; set; } = 0;
}