namespace ChauThanhEV.Models
{
    // Dòng hiển thị trong bảng danh sách trụ sạc, đã gộp sẵn thông tin trạm + cổng.
    public class ChargerRow
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int StationId { get; set; }
        public string StationName { get; set; } = "";
        public double PowerKw { get; set; }
        public ChargerStatus Status { get; set; }
        public bool HasFault { get; set; }
        public int ConnectorCount { get; set; }
        public int AvailableCount { get; set; }
        public int ChargingCount { get; set; }
        public int FaultCount { get; set; }
        public DateTime InstallDate { get; set; }
        public List<Connector> Connectors { get; set; } = new();

        // Trạng thái hiển thị dùng cho bộ lọc: Online / Offline / Fault / Maintenance
        public string FilterStatus => Status == ChargerStatus.Offline ? "Offline" : Status == ChargerStatus.Maintenance ? "Maintenance" : (HasFault ? "Fault" : "Online");
    }

    public class ChargerListViewModel
    {
        public PagedResult<ChargerRow> Result { get; set; } = new();
        public string? Keyword { get; set; }
        public string? StatusFilter { get; set; }
        public List<Station> Stations { get; set; } = new();
    }

    public class ChargerFormModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int StationId { get; set; }
        public double PowerKw { get; set; } = 60;
        public int ConnectorCount { get; set; } = 2;
        public ChargerStatus Status { get; set; } = ChargerStatus.Online;
    }
}
