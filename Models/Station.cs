namespace ChauThanhEV.Models
{
    public class Station
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public string TimeZone { get; set; } = "Asia/Ho_Chi_Minh";
        public decimal DefaultElectricityPrice { get; set; } = 3800m;
        public bool Active { get; set; } = true;
    }
}
