using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy_API.Models.Order
{
    public class OrderDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderDetailId { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Product.Product? Product { get; set; }

        [StringLength(350)]
        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        // ✅ Sửa double → decimal + numeric(18,2)
        [Column(TypeName = "numeric(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal TotalPrice { get; set; }
    }
}