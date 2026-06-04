namespace Pharmacy_API.Models.Product
{
    public class ProductFilterDto
    {
        public int Page { get; set; } = 1; // Trang hiện tại
        public int PageSize { get; set; } = 10; // Số sản phẩm mỗi trang

        public string? Keyword { get; set; } // Từ khóa tìm kiếm

        public int? CategoryId { get; set; } // Lọc theo danh mục
        public int? BrandId { get; set; } // Lọc theo thương hiệu
        public int? BrandOriginId { get; set; } // Lọc theo xuất xứ (đổi từ CountryId)
        public int? UnitId { get; set; } // Lọc theo đơn vị

        // ✅ Sửa double? → decimal?
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public StockStatus? StockStatus { get; set; } // Lọc theo trạng thái tồn kho (Còn hàng/Hết hàng)
    }
}