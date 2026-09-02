namespace ChauThanhEV.Models
{
    public class StatCard
    {
        public string IconKey { get; set; } = "";
        public string IconBg { get; set; } = "";
        public string Title { get; set; } = "";
        public string Value { get; set; } = "";
        public string Unit { get; set; } = "";
    }

    public class OverviewCard
    {
        public string IconKey { get; set; } = "";
        public string IconBg { get; set; } = "";
        public string Title { get; set; } = "";
        public string Value { get; set; } = "";
        public string CompareLabel { get; set; } = "";
        public decimal ChangePercent { get; set; }
        public bool IsPositive => ChangePercent >= 0;
    }

    public class ChartSeriesData
    {
        public List<string> Labels { get; set; } = new();
        public List<double> Values { get; set; } = new();
    }

    public class DashboardViewModel
    {
        public string AdminName { get; set; } = "admin";
        public string AdminRole { get; set; } = "Quản trị viên";

        // Trạng thái trụ / cổng sạc (tính trực tiếp từ MockDataService)
        public List<StatCard> StationStats { get; set; } = new();
        public int AvailableConnectors { get; set; }
        public int ChargingConnectors { get; set; }
        public int FaultConnectors { get; set; }

        // Tổng quan hôm nay / tháng này
        public string TodayGmvValue { get; set; } = "";
        public string TodayGmvCompare { get; set; } = "";
        public decimal TodayGmvChange { get; set; }
        public List<OverviewCard> TodayOverview { get; set; } = new();

        public string MonthGmvValue { get; set; } = "";
        public string MonthGmvCompare { get; set; } = "";
        public decimal MonthGmvChange { get; set; }
        public List<OverviewCard> MonthOverview { get; set; } = new();

        // Dữ liệu biểu đồ cho 3 khoảng thời gian: Hôm nay / 7 ngày qua / 30 ngày qua
        public ChartSeriesData RevenueToday { get; set; } = new();
        public ChartSeriesData Revenue7Days { get; set; } = new();
        public ChartSeriesData Revenue30Days { get; set; } = new();

        public ChartSeriesData ActiveUsersToday { get; set; } = new();
        public ChartSeriesData ActiveUsers7Days { get; set; } = new();
        public ChartSeriesData ActiveUsers30Days { get; set; } = new();

        // Chỉ số vận hành trực tiếp & phiên sạc gần nhất
        public int TotalStations { get; set; }
        public int TotalChargers { get; set; }
        public int TotalConnectors { get; set; }
        public double UtilizationRate { get; set; }
        public double ActivePowerKw { get; set; }
        public List<DashboardRecentSession> RecentSessions { get; set; } = new();
    }

    public class DashboardRecentSession
    {
        public string Code { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string StationName { get; set; } = "";
        public double EnergyKwh { get; set; }
        public string FormattedAmount { get; set; } = "";
        public ChargingOrderStatus Status { get; set; }
        public DateTime StartTime { get; set; }
    }
}
