using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using asp.Data;
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

        // 1. Lấy danh sách giao dịch (Ưu tiên mới nhất lên đầu)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> Get()
        {
            var list = await _transactionCollection.Find(_ => true)
                .SortByDescending(t => t.Date)
                .ToListAsync();
            return Ok(list);
        }

        // 2. Tạo giao dịch mới (Kèm Fix lỗi TC07)
        [HttpPost]
        public async Task<IActionResult> Post(Transaction transaction)
        {
            // KIỂM TRA LOGIC TC07: BĐS này đã bị chốt hay chưa?
            var existing = await _transactionCollection.Find(t =>
                t.PropertyId == transaction.PropertyId &&
                (t.Status == "Đang xử lý" || t.Status == "Hoàn tất")
            ).FirstOrDefaultAsync();

            if (existing != null)
            {
                return BadRequest(new { message = "LỖI: Bất động sản này đã được giao dịch, không thể thao tác!" });
            }

            transaction.Date = System.DateTime.Now;
            if (string.IsNullOrEmpty(transaction.Status))
            {
                transaction.Status = "Đang xử lý";
            }

            await _transactionCollection.InsertOneAsync(transaction);
            return Ok(new { message = "Tạo giao dịch thành công", data = transaction });
        }

        // 3. Cập nhật trạng thái giao dịch (Hoàn tất / Hủy)
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] string newStatus)
        {
            var filter = Builders<Transaction>.Filter.Eq(t => t.Id, id);
            var update = Builders<Transaction>.Update.Set(t => t.Status, newStatus);

            var result = await _transactionCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0) return NotFound(new { message = "Không tìm thấy giao dịch" });
            return Ok(new { message = "Cập nhật trạng thái thành công" });
        }
    }
}