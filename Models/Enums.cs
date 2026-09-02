namespace ChauThanhEV.Models
{
    public enum ChargerStatus
    {
        Online,
        Offline,
        Maintenance
    }

    // Trạng thái vận hành của từng cổng sạc (connector).
    public enum ConnectorStatus
    {
        Available,
        Charging,
        Fault,
        Maintenance
    }

    public enum ChargingOrderStatus
    {
        Completed,   // Hoàn thành
        Charging,    // Đang sạc
        Cancelled,   // Đã hủy
        Failed,      // Thất bại
        Abnormal     // Bất thường
    }

    public enum TopUpStatus
    {
        Success,     // Thành công
        Processing,  // Đang xử lý
        Failed       // Thất bại
    }

    public enum CustomerStatus
    {
        Active,
        Locked
    }

    public enum StationStatus
    {
        Active,
        Inactive
    }

    public enum CardStatus
    {
        Active,
        Inactive
    }

    public enum ReservationStatus
    {
        Active,
        Expired,
        Cancelled
    }

    public enum FaultSeverity
    {
        Low,
        Medium,
        High
    }

    public enum FaultStatus
    {
        New,        // Mới
        Processing, // Đang xử lý
        Resolved    // Đã xử lý
    }

    public enum AbnormalOrderType
    {
        TransactionStuck,   // Giao dịch bị treo
        WrongAmount,        // Sai số tiền
        PaymentFailed,      // Trừ tiền nhưng giao dịch thất bại
        DuplicateCharge     // Trừ tiền hai lần
    }

    public enum AbnormalOrderStatus
    {
        Pending,   // Chờ xử lý
        Resolved   // Đã xử lý
    }

    public enum SourceOrderType
    {
        Charging,  // Đơn hàng sạc
        TopUp      // Đơn hàng nạp tiền
    }

    // Khoảng thời gian dùng cho các bộ lọc biểu đồ / thống kê trên Dashboard.
    public enum DashboardRange
    {
        Today,
        Last7Days,
        Last30Days
    }
}
