    using Pharmacy_API.Models.Category;
    using Pharmacy_API.Models.Product;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.ComponentModel.DataAnnotations;

    namespace Pharmacy_API.Dtos.Category
    {
        public class CategoryDto
        {
            public int CategoryId { get; set; }
            public int? ParentId { get; set; }
            //public Models.Category.Category? Parent { get; set; }
            public string? CategoryName { get; set; }
            public string? CategoryAlias { get; set; }
            public string? CategoryImage { get; set; }
            public string? Sort { get; set; }
            [NotMapped]
            public IFormFile? ImageFile { get; set; }
            public List<Models.Category.Category>? Children { get; set; }
            public List<Models.Product.Product>? Products { get; set; }
            // ✅ Thêm field này
            public int ProductCount { get; set; }
    }
    }
