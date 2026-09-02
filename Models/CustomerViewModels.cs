namespace ChauThanhEV.Models
{
    public class CustomerListViewModel
    {
        public PagedResult<Customer> Result { get; set; } = new();
        public string? Keyword { get; set; }
        public string? StatusFilter { get; set; }
    }

    public class CustomerFormModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public decimal WalletBalance { get; set; }
        public CustomerStatus Status { get; set; } = CustomerStatus.Active;
    }

    public class AdjustBalanceModel
    {
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }       // Số tiền điều chỉnh (có thể âm để trừ)
        public string Note { get; set; } = "";
    }
}
