using Microsoft.AspNetCore.Mvc;
using Pharmacy_API.Dtos.Order;
using Pharmacy_API.Services.Order;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int Page = 1,
        [FromQuery] int PageSize = 20,
        [FromQuery] string? Keyword = null,
        [FromQuery] string? Status = null,
        [FromQuery] string? PaymentStatus = null)
    {
        var result = await _orderService.GetOrdersAsync(Page, PageSize, Keyword, Status, PaymentStatus);
        return Ok(new { data = result.Data, totalRecords = result.TotalRecords });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var order = await _orderService.CreateOrderAsync(dto);
        return Ok(order);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDto dto)
    {
        var order = await _orderService.UpdateOrderAsync(id, dto);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var success = await _orderService.DeleteOrderAsync(id);
        if (!success) return NotFound();
        return Ok(new { success = true });
    }
}