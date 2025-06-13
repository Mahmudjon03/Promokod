namespace PromoRandom.ViewModels
{
    public class WinnerStatModel
    {
        public string GiveawayName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<PrizeWinner> Winners { get; set; } = new();

    }
}
