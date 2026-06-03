namespace Pharmacy_API.Dtos.Payment
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public string? OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string? Content { get; set; }
        public string? AccountNumber { get; set; }
        public string? BankName { get; set; }
        public string? BankCode { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public string? Status { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Note { get; set; }
    }

    public class PaymentCreateDto
    {
        public decimal Amount { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public string? Content { get; set; }
        public string? PaymentMethod { get; set; } = "bank_transfer";
    }

    public class PaymentUpdateDto
    {
        public string? Status { get; set; }
        public string? Note { get; set; }
    }

    public class SePayWebhookDto
    {
        public string? Id { get; set; }
        public string? Gateway { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string? AccountNumber { get; set; }
        public string? Code { get; set; }
        public string? Content { get; set; }
        public string? TransferType { get; set; }
        public decimal? TransferAmount { get; set; }
        public decimal? Accumulated { get; set; }
        public string? ReferenceCode { get; set; }
        public string? TransactionId { get; set; }
        public string? Description { get; set; }
    }
}