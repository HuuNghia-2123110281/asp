using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using asp.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IMongoCollection<Transaction> _transactionCollection;

        public TransactionController(IMongoDatabase database)
        {
            _transactionCollection = database.GetCollection<Transaction>("Transactions");
        }

        // 1. LẤY TẤT CẢ
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetAll() =>
            Ok(await _transactionCollection.Find(_ => true).ToListAsync());

        // 2. LẤY CHI TIẾT THEO ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetById(string id)
        {
            var tran = await _transactionCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (tran == null) return NotFound(new { message = "Không tìm thấy giao dịch!" });
            return Ok(tran);
        }

        // 3. THÊM MỚI 
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Transaction transaction)
        {
            await _transactionCollection.InsertOneAsync(transaction);
            return Ok(new { message = "Thêm giao dịch thành công bởi Nguyen Huu Nghia!", data = transaction });
        }

        // 4. SỬA TOÀN BỘ THÔNG TIN GIAO DỊCH
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Transaction transactionIn)
        {
            var existing = await _transactionCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (existing == null) return NotFound(new { message = "Không tìm thấy giao dịch!" });

            transactionIn.Id = existing.Id;
            await _transactionCollection.ReplaceOneAsync(t => t.Id == id, transactionIn);
            return Ok(new { message = "Cập nhật giao dịch thành công bởi Nguyen Huu Nghia!" });
        }

        // 4.1 SỬA NHANH TRẠNG THÁI
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] string status)
        {
            var update = Builders<Transaction>.Update.Set("Status", status);
            var result = await _transactionCollection.UpdateOneAsync(t => t.Id == id, update);

            if (result.ModifiedCount > 0)
                return Ok(new { message = "Cập nhật trạng thái thành công bởi Nguyen Huu Nghia!" });

            return NotFound(new { message = "Không tìm thấy giao dịch!" });
        }

        // 5. XÓA GIAO DỊCH
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _transactionCollection.DeleteOneAsync(t => t.Id == id);
            if (result.DeletedCount > 0)
                return Ok(new { message = "Đã xóa giao dịch thành công bởi Nguyen Huu Nghia!" });

            return NotFound(new { message = "Không tìm thấy giao dịch!" });
        }
    }
}