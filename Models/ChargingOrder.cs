namespace ChauThanhEV.Models
{
    public class ChargingOrder
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";      // Mã đơn, vd: SC-000123
        public int CustomerId { get; set; }
        public int ChargerId { get; set; }
        public int ConnectorId { get; set; }
        public int StationId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double EnergyKwh { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Ví điện tử";
        public ChargingOrderStatus Status { get; set; } = ChargingOrderStatus.Completed;
    }
}
