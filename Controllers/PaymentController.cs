using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharmacy_API.Context;
using Pharmacy_API.Dtos.Payment;
using Pharmacy_API.Models.Payment;

namespace Pharmacy_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly AccountContext _context;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(AccountContext context, ILogger<PaymentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Payment
        [HttpGet]
        public async Task<IActionResult> GetPayments(
            [FromQuery] int Page = 1,
            [FromQuery] int PageSize = 20,
            [FromQuery] string? Keyword = null,
            [FromQuery] string? Status = null)
        {
            var query = _context.Payments.AsQueryable();

            if (!string.IsNullOrEmpty(Keyword))
                query = query.Where(p =>
                    (p.OrderCode != null && p.OrderCode.Contains(Keyword)) ||
                    (p.CustomerPhone != null && p.CustomerPhone.Contains(Keyword)));

            if (!string.IsNullOrEmpty(Status) && Status != "all")
                query = query.Where(p => p.Status == Status);

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((Page - 1) * PageSize)
                .Take(PageSize)
                .Select(p => new PaymentDto
                {
                    PaymentId = p.PaymentId,
                    OrderCode = p.OrderCode,
                    Amount = p.Amount,
                    CustomerName = p.CustomerName,
                    CustomerPhone = p.CustomerPhone,
                    Status = p.Status,
                    PaymentMethod = p.PaymentMethod,
                    TransactionId = p.TransactionId,
                    CreatedAt = p.CreatedAt,
                    PaymentDate = p.PaymentDate,
                    Note = p.Note,
                })
                .ToListAsync();

            return Ok(new { data, totalRecords = total });
        }

        // GET: api/Payment/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });
            return Ok(payment);
        }

        // POST: api/Payment
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentCreateDto dto)
        {
            var payment = new Payment
            {
                OrderCode = $"DH{DateTime.Now:yyMMddHHmmss}{new Random().Next(100, 999)}",
                Amount = dto.Amount,
                CustomerName = dto.CustomerName,
                CustomerPhone = dto.CustomerPhone,
                CustomerEmail = dto.CustomerEmail,
                Content = dto.Content ?? $"DH{DateTime.Now:yyMMddHHmmss}",
                PaymentMethod = dto.PaymentMethod ?? "bank_transfer",
                CreatedAt = DateTime.UtcNow,
                Status = "pending"
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ Created payment: {payment.OrderCode}");
            return Ok(payment);
        }

        // PUT: api/Payment/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] PaymentUpdateDto dto)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            if (dto.Status != null) payment.Status = dto.Status;
            if (dto.Note != null) payment.Note = dto.Note;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(payment);
        }

        // DELETE: api/Payment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        // POST: api/Payment/webhook (SePay callback)
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookDto dto)
        {
            _logger.LogInformation($"🔔 SePay Webhook: {System.Text.Json.JsonSerializer.Serialize(dto)}");

            var orderCode = dto.Content?.Trim();
            if (string.IsNullOrEmpty(orderCode))
                return Ok(new { success = false, message = "Empty content" });

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderCode == orderCode);

            if (payment != null)
            {
                payment.Status = "completed";
                payment.TransactionId = dto.TransactionId ?? dto.ReferenceCode;
                payment.PaymentDate = dto.TransactionDate ?? DateTime.UtcNow;
                payment.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Payment completed: {orderCode}");
            }
            else
            {
                _logger.LogWarning($"⚠️ Order not found: {orderCode}");
            }

            return Ok(new { success = true });
        }
    }
}