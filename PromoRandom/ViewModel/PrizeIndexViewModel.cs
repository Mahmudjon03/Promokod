using PromoRandom.Models;

namespace PromoRandom.ViewModel
{
    public class PrizeIndexViewModel
    {
        public List<Prize> Prizes { get; set; }
        public List<PrizeWithPromoAndUser> PrizeWithPromoAndUsers { get; set; }
    }
}
