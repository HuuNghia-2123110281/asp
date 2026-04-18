using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using asp.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IMongoCollection<Payment> _paymentCollection;

        public PaymentController(IMongoDatabase database)
        {
            _paymentCollection = database.GetCollection<Payment>("Payments");
        }

        // 1. Lấy toàn bộ lịch sử thanh toán của 1 Giao dịch
        [HttpGet("transaction/{transId}")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetByTransaction(string transId)
        {
            var list = await _paymentCollection.Find(p => p.TransactionId == transId)
                .SortByDescending(p => p.PaymentDate)
                .ToListAsync();
            return Ok(list);
        }

        // 2. Thêm một đợt thanh toán mới
        [HttpPost]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (payment.Amount <= 0) return BadRequest(new { message = "Số tiền phải lớn hơn 0" });

            payment.PaymentDate = System.DateTime.Now;
            await _paymentCollection.InsertOneAsync(payment);

            return Ok(new { message = "Ghi nhận thanh toán thành công!", data = payment });
        }
    }
}