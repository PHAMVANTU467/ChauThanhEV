namespace ChauThanhEV.Models
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }

        public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);

        public static PagedResult<T> Create(IEnumerable<T> source, int page, int pageSize)
        {
            var all = source.ToList();
            page = page < 1 ? 1 : page;
            var result = new PagedResult<T>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = all.Count,
                Items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList()
            };
            return result;
        }
    }
}
