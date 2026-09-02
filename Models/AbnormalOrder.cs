namespace ChauThanhEV.Models
{
    public class AbnormalOrder
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";           // Mã ghi nhận, vd: BT-0001
        public SourceOrderType OrderType { get; set; }
        public string OrderCode { get; set; } = "";      // Mã đơn hàng gốc (sạc hoặc nạp tiền)
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public AbnormalOrderType Type { get; set; }
        public string Note { get; set; } = "";
        public AbnormalOrderStatus Status { get; set; } = AbnormalOrderStatus.Pending;
        public DateTime DetectedAt { get; set; }
        public string? ResolutionNote { get; set; }
    }
}
