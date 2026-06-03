using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Pharmacy_API.Models.Category;
using Pharmacy_API.Models.Brand;
using Pharmacy_API.Models.Unit;
using Pharmacy_API.Models.Country;

namespace Pharmacy_API.Models.Product
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        /// <summary>
        /// Mã sản phẩm (vd: P01045)
        /// </summary>
        [StringLength(50)]
        public string? ProductCode { get; set; }

        [Required]
        [StringLength(350)]
        public string? ProductName { get; set; }

        [Range(0, double.MaxValue)]
        public double Price { get; set; }

        public string? Images { get; set; }

        // ✅ Tăng cho CKEditor (có thể chứa ảnh)
        [StringLength(50000)]
        public string? Description { get; set; }

        [StringLength(350)]
        public string? NameAlias { get; set; }

        public DateTime ProductionDate { get; set; }

        [Range(0, double.MaxValue)]
        public double Sale { get; set; }

        public int CategoryId { get; set; }
        public Category.Category? Category { get; set; }

        public int BrandId { get; set; }
        public Brand.Brand? Brand { get; set; }

        public int UnitId { get; set; }
        public Unit.Unit? Unit { get; set; }

        // ✅ Tăng cho CKEditor
        [StringLength(10000)]
        public string? SortDescription { get; set; }

        [StringLength(350)]
        public string? DosageForm { get; set; }

        [StringLength(350)]
        public string? Packaging { get; set; }

        public int BrandOriginId { get; set; }
        public Country.Country? Country { get; set; }

        public int ManufacturerId { get; set; }
        public Country.Country? Manufacturer { get; set; }

        // ✅ Tăng cho CKEditor
        [StringLength(20000)]
        public string? Ingredients { get; set; }

        // ✅ Tăng cho CKEditor
        [StringLength(20000)]
        public string? Usage { get; set; }

        // ✅ Tăng cho CKEditor
        [StringLength(20000)]
        public string DosageAndAdministration { get; set; } = string.Empty;

        // ✅ Tăng cho CKEditor
        [StringLength(10000)]
        public string SideEffects { get; set; } = string.Empty;

        // ✅ Tăng cho CKEditor
        [StringLength(10000)]
        public string Precautions { get; set; } = string.Empty;

        // ✅ Tăng cho CKEditor
        [StringLength(5000)]
        public string Storage { get; set; } = string.Empty;

        public string? Sort { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public StockStatus StockStatus { get; set; } = StockStatus.InStock;

        public bool IsActive { get; set; }

        public DateTime? ActiveFrom { get; set; }
    }
}