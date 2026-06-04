using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy_API.Models.Order
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        [StringLength(50)]
        public string OrderCode { get; set; } = string.Empty;

        public int? CustomerId { get; set; }

        [StringLength(200)]
        public string? CustomerName { get; set; }

        [StringLength(20)]
        public string? CustomerPhone { get; set; }

        [StringLength(200)]
        public string? CustomerEmail { get; set; }

        [StringLength(500)]
        public string? ShippingAddress { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal Discount { get; set; }

        // ✅ THÊM MỚI: Phí vận chuyển
        [Column(TypeName = "numeric(18,2)")]
        public decimal ShippingFee { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal FinalAmount { get; set; }

        // ✅ THÊM MỚI: Phương thức thanh toán
        [StringLength(50)]
        public string? PaymentMethod { get; set; }  // COD, VNPay, Momo, BankTransfer

        [StringLength(20)]
        public string Status { get; set; } = "pending";

        [StringLength(20)]
        public string PaymentStatus { get; set; } = "unpaid";

        public int? PaymentId { get; set; }
        public Payment.Payment? Payment { get; set; }

        [StringLength(1000)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public List<OrderDetail> OrderDetails { get; set; } = new();
    }
}