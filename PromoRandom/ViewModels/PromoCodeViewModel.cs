using PromoRandom.Models;

namespace PromoRandom.ViewModels;

public class PromoCodeViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public int OrderId { get; set; }
    public string UserName { get; set; } = "";
    public string GiveName { get; set; } = "";
    public string Status { get; set; } = "active";
    public DateTime SubmittedAt { get; set; }

    // eti dannie budut polnosenno pokazaksya v modal
    public Order Order { get; set; }
    public User User { get; set; }
    public Give Give { get; set; }
    public Prize Prize { get; set; }
    public string PrizeName { get; internal set; }
}


public class PromoCodeFilter
{
    public string? Search { get; set; }
    public int? GiveId { get; set; }
    public long? UserChatId { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int? OrderId { get; internal set; }
    public string? PrizeName { get; internal set; }
}

public class PromoCodeResult
{
    public List<PromoCodeViewModel> PromoCodes { get; set; }
    public int TotalCount { get; set; }
}