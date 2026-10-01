using System.ComponentModel.DataAnnotations;
namespace ECommerceAPI.DTOs.Requests
{
    public sealed class ProductFilterRequestDTO
    {
        [StringLength(100, ErrorMessage = "Search Term cannot exceed 100 characters.")]
        public string? SearchTerm { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Category Id must be greater than 0.")]
        public int? CategoryId { get; set; }

        [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Minimum Price cannot be negative.")]
        public decimal? MinPrice { get; set; }

        [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Maximum Price cannot be negative.")]
        public decimal? MaxPrice { get; set; }
        public bool InStockOnly { get; set; }

        [RegularExpression(@"(?i)^(name|price)$", ErrorMessage = "Sort By must be either Name or Price.")]
        public string? SortBy { get; set; } = "Name";

        [RegularExpression(@"(?i)^(asc|desc)$", ErrorMessage = "Sort Direction must be Asc or Desc.")]
        public string? SortDirection { get; set; } = "Asc";

        [Range(1, int.MaxValue, ErrorMessage = "Page Number must be greater than 0.")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page Size must be between 1 and 100.")]
        public int PageSize { get; set; } = 10;
    }
}
