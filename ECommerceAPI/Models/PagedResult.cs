namespace ECommerceAPI.Models
{
    public sealed class PagedResult<T>
    {
        // 1, 10
        // 100
        public int PageNumber { get; set; } //1
        public int PageSize { get; set; }  //10
        public int TotalCount { get; set; } //100
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize); //10
        public List<T> Items { get; set; } = new List<T>();
    }
}
