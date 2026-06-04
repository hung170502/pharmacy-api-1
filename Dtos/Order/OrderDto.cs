namespace Pharmacy_API.Dtos.Order
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public string? OrderCode { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }          // ✅ Thêm
        public string? ShippingAddress { get; set; }       // ✅ Thêm
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }              // ✅ Thêm
        public decimal ShippingFee { get; set; }           // ✅ Thêm
        public decimal FinalAmount { get; set; }
        public string? PaymentMethod { get; set; }         // ✅ Thêm
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public string? Note { get; set; }                  // ✅ Thêm
        public DateTime CreatedAt { get; set; }
        public int ItemCount { get; set; }
    }

    public class CreateOrderDto
    {
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public string? ShippingAddress { get; set; }
        public decimal? Discount { get; set; }
        public decimal ShippingFee { get; set; }           // ✅ Thêm
        public string? PaymentMethod { get; set; }         // ✅ Thêm
        public string? Note { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateOrderDto
    {
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }         // ✅ Thêm
        public string? Note { get; set; }
    }
}