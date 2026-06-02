using Pharmacy_API.Models.Product;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_API.Dtos.Product
{
    public class ProductRequestDto
    {
        [Required]
        [StringLength(350)]
        public string ProductName { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public double Price { get; set; }

        public string? Images { get; set; }

        [StringLength(3000)]
        public string? Description { get; set; }

        [StringLength(350)]
        public string? NameAlias { get; set; }

        public DateTime ProductionDate { get; set; }

        [Range(0, double.MaxValue)]
        public double Sale { get; set; }

        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int UnitId { get; set; }

        [StringLength(1000)]
        public string? SortDescription { get; set; }

        [StringLength(350)]
        public string? DosageForm { get; set; }

        [StringLength(350)]
        public string? Packaging { get; set; }

        // SỬA: Đổi tên cho khớp Model
        public int BrandOriginId { get; set; }  // Thay vì CountryId

        // SỬA: Đổi thành int và thêm Id
        public int ManufacturerId { get; set; }  // Thay vì Manufacturer (string)

        [StringLength(350)]
        public string? Ingredients { get; set; }

        [StringLength(350)]
        public string? Usage { get; set; }

        [StringLength(350)]
        public string DosageAndAdministration { get; set; } = string.Empty;

        [StringLength(350)]
        public string SideEffects { get; set; } = string.Empty;

        [StringLength(350)]
        public string Precautions { get; set; } = string.Empty;

        [StringLength(350)]
        public string Storage { get; set; } = string.Empty;

        public string? Sort { get; set; }

        public StockStatus StockStatus { get; set; } = StockStatus.InStock;

        public bool IsActive { get; set; } = true;

        public DateTime? ActiveFrom { get; set; }
    }
}