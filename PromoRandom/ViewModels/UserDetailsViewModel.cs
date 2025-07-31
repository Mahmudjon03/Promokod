using PromoRandom.Models;

namespace PromoRandom.ViewModels;

// UserDetailsViewModel.cs
public class UserDetailsViewModel
{
    public User User { get; set; }
    public List<PromoCodeViewModel> PromoCodes { get; set; }
    public List<Order> Orders { get; set; }
}