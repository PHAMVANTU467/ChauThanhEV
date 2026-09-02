namespace ChauThanhEV.Models
{
    public class PagerViewModel
    {
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public string Controller { get; set; } = "";
        public string Action { get; set; } = "Index";
        public Dictionary<string, string> RouteValues { get; set; } = new();
    }
}
