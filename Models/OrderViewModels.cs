namespace ChauThanhEV.Models
{
    public class ChargingOrderRow
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string CustomerCode { get; set; } = "";
        public string ChargerCode { get; set; } = "";
        public string ConnectorCode { get; set; } = "";
        public string StationName { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double EnergyKwh { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public ChargingOrderStatus Status { get; set; }
    }

    public class TopUpOrderRow
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string CustomerCode { get; set; } = "";
        public decimal Amount { get; set; }
        public string Method { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public TopUpStatus Status { get; set; }
    }

    public class OrderListViewModel
    {
        public string Tab { get; set; } = "charging"; // charging | topup
        public string? Keyword { get; set; }
        public string? StatusFilter { get; set; }

        public PagedResult<ChargingOrderRow> ChargingResult { get; set; } = new();
        public PagedResult<TopUpOrderRow> TopUpResult { get; set; } = new();
    }
}
