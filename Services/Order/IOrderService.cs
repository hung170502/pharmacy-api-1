using Pharmacy_API.Dtos.Order;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Order
{
    public interface IOrderService
    {
        Task<PagedDto<OrderDto>> GetOrdersAsync(int page, int pageSize, string? keyword, string? status, string? paymentStatus);
        Task<Models.Order.Order?> GetOrderByIdAsync(int id);
        Task<Models.Order.Order> CreateOrderAsync(CreateOrderDto dto);
        Task<Models.Order.Order?> UpdateOrderAsync(int id, UpdateOrderDto dto);
        Task<bool> DeleteOrderAsync(int id);
    }
}