namespace ChauThanhEV.Models
{
    public class Charger
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";       // Mã trụ, vd: TS-001
        public string Name { get; set; } = "";
        public int StationId { get; set; }
        public double PowerKw { get; set; }
        public ChargerStatus Status { get; set; } = ChargerStatus.Online;
        public DateTime InstallDate { get; set; }

        public List<Connector> Connectors { get; set; } = new();
    }
}
