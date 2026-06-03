using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy_API.Models.Payment
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        [StringLength(50)]
        public string? OrderCode { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Content { get; set; }

        [StringLength(50)]
        public string? AccountNumber { get; set; }

        [StringLength(100)]
        public string? BankName { get; set; }

        [StringLength(50)]
        public string? BankCode { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "pending";

        [StringLength(100)]
        public string? TransactionId { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; } = "bank_transfer";

        public DateTime? PaymentDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(1000)]
        public string? Note { get; set; }

        public int? CustomerId { get; set; }

        [StringLength(200)]
        public string? CustomerName { get; set; }

        [StringLength(20)]
        public string? CustomerPhone { get; set; }

        [StringLength(200)]
        public string? CustomerEmail { get; set; }
    }
}