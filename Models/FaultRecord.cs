namespace ChauThanhEV.Models
{
    public class FaultRecord
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";       // Mã lỗi, vd: LOI-0001
        public int ChargerId { get; set; }
        public int? ConnectorId { get; set; }
        public string Description { get; set; } = "";
        public FaultSeverity Severity { get; set; } = FaultSeverity.Medium;
        public FaultStatus Status { get; set; } = FaultStatus.New;
        public DateTime ReportedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
