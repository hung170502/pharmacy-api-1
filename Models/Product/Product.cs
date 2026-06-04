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

        // ✅ ĐÃ SỬA: double → decimal, dùng cho PostgreSQL
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "numeric(18,2)")]
        public decimal Price { get; set; }

        public string? Images { get; set; }

        [StringLength(50000)]
        public string? Description { get; set; }

        [StringLength(350)]
        public string? NameAlias { get; set; }

        public DateTime ProductionDate { get; set; }

        // ✅ ĐÃ SỬA: double → decimal
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "numeric(18,2)")]
        public decimal Sale { get; set; }

        public int CategoryId { get; set; }
        public Category.Category? Category { get; set; }

        public int BrandId { get; set; }
        public Brand.Brand? Brand { get; set; }

        public int UnitId { get; set; }
        public Unit.Unit? Unit { get; set; }

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

        [StringLength(20000)]
        public string? Ingredients { get; set; }

        [StringLength(20000)]
        public string? Usage { get; set; }

        [StringLength(20000)]
        public string DosageAndAdministration { get; set; } = string.Empty;

        [StringLength(10000)]
        public string SideEffects { get; set; } = string.Empty;

        [StringLength(10000)]
        public string Precautions { get; set; } = string.Empty;

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