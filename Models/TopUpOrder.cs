namespace ChauThanhEV.Models
{
    public class TopUpOrder
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";     // Mã đơn, vd: NT-000045
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = "Chuyển khoản ngân hàng";
        public DateTime CreatedAt { get; set; }
        public TopUpStatus Status { get; set; } = TopUpStatus.Success;
    }
}
