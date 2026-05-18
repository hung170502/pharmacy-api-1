namespace Pharmacy_API.Filters.Product
{
    public class ProductFilter
    {
        public int Page { get; set; } = 1; // Trang hiện tại
        public int PageSize { get; set; } = 10; // Số sản phẩm mỗi trang

        public string? Keyword { get; set; } // Từ khóa tìm kiếm

        public int? CategoryId { get; set; } // Lọc theo danh mục
        public int? BrandId { get; set; } // Lọc theo thương hiệu
        public int? CountryId { get; set; } // Lọc theo xuất xứ
        public int? UnitId { get; set; } // Lọc theo đơn vị

        public double? MinPrice { get; set; } // Giá từ
        public double? MaxPrice { get; set; } // Giá đến
    }
}
