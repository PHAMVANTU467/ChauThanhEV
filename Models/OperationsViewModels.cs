namespace ChauThanhEV.Models
{
    public class FaultRow
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string ChargerCode { get; set; } = "";
        public string ChargerName { get; set; } = "";
        public string? ConnectorCode { get; set; }
        public string Description { get; set; } = "";
        public FaultSeverity Severity { get; set; }
        public FaultStatus Status { get; set; }
        public DateTime ReportedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class OfflineChargerRow
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string StationName { get; set; } = "";
        public int ConnectorCount { get; set; }
        public DateTime? SinceReportedFault { get; set; }
        public string? Reason { get; set; }
    }

    public class AbnormalOrderRow
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public SourceOrderType OrderType { get; set; }
        public string OrderCode { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string CustomerCode { get; set; } = "";
        public decimal Amount { get; set; }
        public AbnormalOrderType Type { get; set; }
        public string Note { get; set; } = "";
        public AbnormalOrderStatus Status { get; set; }
        public DateTime DetectedAt { get; set; }
        public string? ResolutionNote { get; set; }
    }

    public class OperationsViewModel
    {
        public string Tab { get; set; } = "faults"; // faults | offline | abnormal

        public List<FaultRow> Faults { get; set; } = new();
        public List<OfflineChargerRow> OfflineChargers { get; set; } = new();
        public List<AbnormalOrderRow> AbnormalOrders { get; set; } = new();
    }
}
