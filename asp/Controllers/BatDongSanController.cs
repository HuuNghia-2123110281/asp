using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using asp.Data;

namespace asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BatDongSanController : ControllerBase
    {
        private readonly IMongoCollection<BatDongSan> _bdsCollection;

        public BatDongSanController(IMongoDatabase database)
        {
            // Kết nối tới collection 'properties' theo yêu cầu tài liệu
            _bdsCollection = database.GetCollection<BatDongSan>("BatDongSans");
        }

        // 1. Lấy danh sách + Phân trang
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BatDongSan>>> GetBatDongSans(int page = 1, int pageSize = 10)
        {
            var list = await _bdsCollection.Find(_ => true)
                .SortByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Ok(list);
        }

        // 2. Xem chi tiết theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<BatDongSan>> GetBatDongSan(string id)
        {
            var batDongSan = await _bdsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (batDongSan == null) return NotFound(new { message = "Không tìm thấy!" });
            return Ok(batDongSan);
        }

        // 3. Thêm mới BĐS (Module 5.1.1 trong tài liệu)
        [HttpPost]
        public async Task<ActionResult<BatDongSan>> PostBatDongSan(BatDongSan batDongSan)
        {
            if (string.IsNullOrEmpty(batDongSan.TieuDe))
                return BadRequest(new { message = "Tên BĐS không được để trống" });

            await _bdsCollection.InsertOneAsync(batDongSan);
            return CreatedAtAction(nameof(GetBatDongSan), new { id = batDongSan.Id }, batDongSan);
        }

        // 4. Sửa BĐS (Module 5.1.2 trong tài liệu)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBatDongSan(string id, BatDongSan updatedBds)
        {
            var result = await _bdsCollection.ReplaceOneAsync(x => x.Id == id, updatedBds);
            if (result.MatchedCount == 0) return NotFound();
            return Ok(updatedBds);
        }

        // 5. Xóa BĐS (Module 5.1.3 trong tài liệu)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBatDongSan(string id)
        {
            var result = await _bdsCollection.DeleteOneAsync(x => x.Id == id);
            if (result.DeletedCount == 0) return NotFound();
            return NoContent();
        }
    }
}