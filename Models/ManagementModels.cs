namespace ChauThanhEV.Models
{
    public class StationFormModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public string TimeZone { get; set; } = "Asia/Ho_Chi_Minh";
        public decimal DefaultElectricityPrice { get; set; } = 3800m;
        public bool Active { get; set; } = true;
    }

    public class StationListViewModel
    {
        public PagedResult<Station> Result { get; set; } = new();
        public string? Keyword { get; set; }
        public string? StatusFilter { get; set; }
    }

    public class TopUpPackage
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal Bonus { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class TopUpPackageFormModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal Bonus { get; set; }
        public bool Active { get; set; } = true;
    }

    public class TopUpPackageListViewModel
    {
        public PagedResult<TopUpPackage> Result { get; set; } = new();
        public string? Keyword { get; set; }
        public string? StatusFilter { get; set; }
    }

    public class RfidCard
    {
        public int Id { get; set; }
        public string CardId { get; set; } = "";
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public CardStatus Status { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ExpiryDate { get; set; } = DateTime.Now.AddMonths(6);
    }

    public class RfidCardFormModel
    {
        public int Id { get; set; }
        public string CardId { get; set; } = "";
        public int UserId { get; set; }
        public DateTime ExpiryDate { get; set; } = DateTime.Now.AddMonths(6);
    }

    public class RfidCardListViewModel
    {
        public PagedResult<RfidCard> Result { get; set; } = new();
        public string? Keyword { get; set; }
        public string? StatusFilter { get; set; }
        public List<Customer> Customers { get; set; } = new();
    }

    public class Reservation
    {
        public int Id { get; set; }
        public string ReservationId { get; set; } = "";
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public int ChargerId { get; set; }
        public string ChargerCode { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ReservationStatus Status { get; set; }
    }

    public class ReservationFormModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ChargerId { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now.AddHours(2);
        public int DurationMinutes { get; set; } = 60;
    }

    public class ReservationListViewModel
    {
        public PagedResult<Reservation> Result { get; set; } = new();
        public string? Keyword { get; set; }
        public string? StatusFilter { get; set; }
        public List<Customer> Customers { get; set; } = new();
        public List<Charger> Chargers { get; set; } = new();
    }

    public class FinancialTransaction
    {
        public int Id { get; set; }
        public string TransactionId { get; set; } = "";
        public DateTime TransactionTime { get; set; } = DateTime.Now;
        public string TransactionType { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? Note { get; set; }
    }

    public class FinancialOverview
    {
        public decimal TotalIncome { get; set; }
        public decimal TodayIncome { get; set; }
        public decimal ThisMonthIncome { get; set; }
        public decimal AvailableBalance { get; set; }
    }

    public class FinancialViewModel
    {
        public FinancialOverview Overview { get; set; } = new();
        public PagedResult<FinancialTransaction> Transactions { get; set; } = new();
        public string? Keyword { get; set; }
    }
}
