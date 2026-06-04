using Microsoft.EntityFrameworkCore;
using Pharmacy_API.Context;
using Pharmacy_API.Dtos.Order;
using Pharmacy_API.Models.Order;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Order
{
    public class OrderService : IOrderService
    {
        private readonly AccountContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(AccountContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedDto<OrderDto>> GetOrdersAsync(int page, int pageSize, string? keyword, string? status, string? paymentStatus)
        {
            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(o => o.OrderCode.Contains(keyword) ||
                    (o.CustomerPhone != null && o.CustomerPhone.Contains(keyword)));

            if (!string.IsNullOrEmpty(status) && status != "all")
                query = query.Where(o => o.Status == status);

            if (!string.IsNullOrEmpty(paymentStatus) && paymentStatus != "all")
                query = query.Where(o => o.PaymentStatus == paymentStatus);

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderDto
                {
                    OrderId = o.OrderId,
                    OrderCode = o.OrderCode,
                    CustomerName = o.CustomerName,
                    CustomerPhone = o.CustomerPhone,
                    CustomerEmail = o.CustomerEmail,          // ✅ Thêm
                    ShippingAddress = o.ShippingAddress,      // ✅ Thêm
                    TotalAmount = o.TotalAmount,
                    Discount = o.Discount,                    // ✅ Thêm
                    ShippingFee = o.ShippingFee,              // ✅ Thêm
                    FinalAmount = o.FinalAmount,
                    PaymentMethod = o.PaymentMethod,          // ✅ Thêm
                    Status = o.Status,
                    PaymentStatus = o.PaymentStatus,
                    Note = o.Note,                            // ✅ Thêm
                    CreatedAt = o.CreatedAt,
                    ItemCount = o.OrderDetails.Count,
                })
                .ToListAsync();

            return new PagedDto<OrderDto>(total, data);
        }

        public async Task<Models.Order.Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }

        public async Task<Models.Order.Order> CreateOrderAsync(CreateOrderDto dto)
        {
            var order = new Models.Order.Order
            {
                OrderCode = await GenerateOrderCodeAsync(),
                CustomerName = dto.CustomerName,
                CustomerPhone = dto.CustomerPhone,
                CustomerEmail = dto.CustomerEmail,
                ShippingAddress = dto.ShippingAddress,
                Note = dto.Note,
                Status = "pending",
                PaymentStatus = "unpaid",
                ShippingFee = dto.ShippingFee,          // ✅ Thêm
                PaymentMethod = dto.PaymentMethod,      // ✅ Thêm
                CreatedAt = DateTime.UtcNow,
            };

            decimal total = 0;

            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null) continue;

                var detail = new OrderDetail
                {
                    ProductId = item.ProductId,
                    ProductName = product.ProductName ?? "",
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * item.Quantity,
                };
                order.OrderDetails.Add(detail);
                total += detail.TotalPrice;
            }

            order.TotalAmount = total;
            order.Discount = dto.Discount ?? 0m;
            order.FinalAmount = total - order.Discount + order.ShippingFee;  // ✅ Cộng phí ship

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ Created order: {order.OrderCode}");
            return order;
        }

        public async Task<Models.Order.Order?> UpdateOrderAsync(int id, UpdateOrderDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return null;

            if (dto.Status != null) order.Status = dto.Status;
            if (dto.PaymentStatus != null) order.PaymentStatus = dto.PaymentStatus;
            if (dto.PaymentMethod != null) order.PaymentMethod = dto.PaymentMethod;  // ✅ Thêm
            if (dto.Note != null) order.Note = dto.Note;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return false;

            _context.OrderDetails.RemoveRange(order.OrderDetails);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<string> GenerateOrderCodeAsync()
        {
            string code;
            do
            {
                code = $"ORD{DateTime.Now:yyMMddHHmmss}{new Random().Next(100, 999)}";
            }
            while (await _context.Orders.AnyAsync(o => o.OrderCode == code));

            return code;
        }
    }
}