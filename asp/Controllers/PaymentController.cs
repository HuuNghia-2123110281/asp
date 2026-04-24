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
        [HttpGet("transaction/{transId}")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetByTransaction(string transId)
        {
            var list = await _paymentCollection.Find(p => p.TransactionId == transId)
                .SortByDescending(p => p.PaymentDate)
                .ToListAsync();
            return Ok(list);
        }
        [HttpPost]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (payment.Amount <= 0) return BadRequest(new { message = "Số tiền phải lớn hơn 0" });

            payment.PaymentDate = System.DateTime.Now;
            await _paymentCollection.InsertOneAsync(payment);

            return Ok(new { message = "Ghi nhận thanh toán thành công!", data = payment });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(string id, [FromBody] Payment paymentIn)
        {
            try
            {
                var existing = await _paymentCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
                if (existing == null) return NotFound(new { message = "Không tìm thấy dữ liệu thanh toán!" });

                paymentIn.Id = existing.Id;
                await _paymentCollection.ReplaceOneAsync(p => p.Id == id, paymentIn);

                return Ok(new { message = "Cập nhật thông tin thanh toán thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Server: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(string id)
        {
            try
            {
                var result = await _paymentCollection.DeleteOneAsync(p => p.Id == id);
                if (result.DeletedCount > 0)
                    return Ok(new { message = "Đã xóa thanh toán thành công!" });

                return NotFound(new { message = "Không tìm thấy dữ liệu thanh toán!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Server: " + ex.Message });
            }
        }
    }
}