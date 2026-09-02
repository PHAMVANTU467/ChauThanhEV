namespace ChauThanhEV.Models
{
    public class Connector
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";   // Mã cổng, vd: TS-001-A
        public int ChargerId { get; set; }
        public string ConnectorType { get; set; } = "CCS2"; // Loại đầu sạc: AC / CCS2 / CHAdeMO...
        public ConnectorStatus Status { get; set; } = ConnectorStatus.Available;
    }
}
