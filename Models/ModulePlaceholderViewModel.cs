namespace ChauThanhEV.Models
{
    public class ModulePlaceholderViewModel
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string[] Columns { get; set; } = Array.Empty<string>();
        public string[] Statuses { get; set; } = Array.Empty<string>();
        public string[] Rows { get; set; } = Array.Empty<string>();
    }
}
