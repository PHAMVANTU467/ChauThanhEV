namespace ChauThanhEV.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";     // Mã khách hàng, vd: KH-0001
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public decimal WalletBalance { get; set; }
        public CustomerStatus Status { get; set; } = CustomerStatus.Active;
        public DateTime RegisteredAt { get; set; }
    }
}
