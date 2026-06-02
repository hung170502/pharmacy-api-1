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
        [StringLength(3000)]
        public string? Description { get; set; }
        [StringLength(350)]
        public string? NameAlias { get; set; }
        public DateTime ProductionDate { get; set; }
        [Range(0, double.MaxValue)]
        public double Sale { get; set; }
        public int CategoryId { get; set; }
        public Category.Category? Category { get; set; }
        public int BrandId { get; set; }
        public Brand.Brand? Brand { get; set; } // thương hiệu
        public int UnitId { get; set; }
        public Unit.Unit? Unit { get; set; } // đơn vị
        [StringLength(1000)]
        public string? SortDescription { get; set; }
        [StringLength(350)]
        public string? DosageForm { get; set; } // dạng bào chế (ví dụ viên, nước, bột)
        [StringLength(350)]
        public string? Packaging { get; set; } // đóng gói (ví dụ hộp ... viên)
        public int BrandOriginId { get; set; } // tên quốc gia sản xuất thuốc
        public Country.Country? Country { get; set; }
        public int ManufacturerId { get; set; } // xuất xứ thương hiệu (như là nơi làm ra sản phẩm)
        public Country.Country? Manufacturer { get; set; } // navigation property thêm vào

        [StringLength(350)]
        public string? Ingredients { get; set; } // thành phần
        [StringLength(350)]
        public string? Usage { get; set; } // tác dụng
        [StringLength(350)]
        public string DosageAndAdministration { get; set; } = string.Empty; // Cách dùng
        [StringLength(350)]
        public string SideEffects { get; set; } = string.Empty; // Tác dụng phụ
        [StringLength(350)]
        public string Precautions { get; set; } = string.Empty; // Lưu ý
        [StringLength(350)]
        public string Storage { get; set; } = string.Empty; // Bảo quản
        public string? Sort { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public StockStatus StockStatus { get; set; } = StockStatus.InStock;
        public bool IsActive { get; set; }
        public DateTime? ActiveFrom { get; set; } // Thời điểm bắt đầu hiển thị (null = hiển thị ngay)
    }
}
