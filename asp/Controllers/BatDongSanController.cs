using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using asp.Data;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BatDongSanController : ControllerBase
    {
        private readonly IMongoCollection<BatDongSan> _bdsCollection;
        private readonly IWebHostEnvironment _env;

        public BatDongSanController(IMongoDatabase database, IWebHostEnvironment env)
        {
            _bdsCollection = database.GetCollection<BatDongSan>("BatDongSans");
            _env = env;
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

        // 3. Thêm mới BĐS
        [HttpPost]
        public async Task<ActionResult<BatDongSan>> PostBatDongSan([FromForm] BatDongSanRequest request)
        {
            if (string.IsNullOrEmpty(request.TieuDe))
                return BadRequest(new { message = "Tên BĐS không được để trống" });

            string? hinhAnhUrl = null;

            try
            {
                if (request.HinhAnhFile != null && request.HinhAnhFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + request.HinhAnhFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.HinhAnhFile.CopyToAsync(fileStream);
                    }

                    hinhAnhUrl = "/images/" + uniqueFileName;
                }

                var batDongSan = new BatDongSan
                {
                    TieuDe = request.TieuDe,
                    LoaiHinh = request.LoaiHinh,
                    DienTich = request.DienTich,
                    PhongNgu = request.PhongNgu,
                    Gia = request.Gia,
                    DiaChi = request.DiaChi,
                    MoTa = request.MoTa,
                    HinhAnhUrl = hinhAnhUrl ?? "https://loremflickr.com/400/300/house"
                };

                await _bdsCollection.InsertOneAsync(batDongSan);
                return CreatedAtAction(nameof(GetBatDongSan), new { id = batDongSan.Id }, batDongSan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lưu BĐS: " + ex.Message });
            }
        }

        // 4. Sửa BĐS
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBatDongSan(string id, [FromForm] BatDongSanRequest request)
        {
            var existingBds = await _bdsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (existingBds == null) return NotFound();

            string? hinhAnhUrl = existingBds.HinhAnhUrl;

            if (request.HinhAnhFile != null && request.HinhAnhFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + request.HinhAnhFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.HinhAnhFile.CopyToAsync(fileStream);
                }
                hinhAnhUrl = "/images/" + uniqueFileName;
            }

            existingBds.TieuDe = request.TieuDe;
            existingBds.LoaiHinh = request.LoaiHinh;
            existingBds.Gia = request.Gia;
            existingBds.DiaChi = request.DiaChi;
            existingBds.MoTa = request.MoTa;
            existingBds.DienTich = request.DienTich;
            existingBds.PhongNgu = request.PhongNgu;
            existingBds.HinhAnhUrl = hinhAnhUrl;

            await _bdsCollection.ReplaceOneAsync(x => x.Id == id, existingBds);
            return Ok(new { message = "Cập nhật thành công!", data = existingBds });
        }

        // 5. Xóa BĐS
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBatDongSan(string id)
        {
            var result = await _bdsCollection.DeleteOneAsync(x => x.Id == id);
            if (result.DeletedCount == 0) return NotFound();
            return NoContent();
        }

        // 6. TÌM KIẾM BĐS
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<BatDongSan>>> SearchBatDongSan([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                var allList = await _bdsCollection.Find(_ => true)
                    .SortByDescending(x => x.Id)
                    .ToListAsync();
                return Ok(allList);
            }
            var filter = Builders<BatDongSan>.Filter.Or(
                Builders<BatDongSan>.Filter.Regex(x => x.TieuDe, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                Builders<BatDongSan>.Filter.Regex(x => x.DiaChi, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                Builders<BatDongSan>.Filter.Regex(x => x.LoaiHinh, new MongoDB.Bson.BsonRegularExpression(keyword, "i"))
            );

            var list = await _bdsCollection.Find(filter)
                .SortByDescending(x => x.Id)
                .ToListAsync();

            return Ok(list);
        }
        // 7. TÌM KIẾM NÂNG CAO
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<BatDongSan>>> FilterBatDongSan(
            [FromQuery] string? keyword,
            [FromQuery] string? loaiHinh,
            [FromQuery] double? minGia,
            [FromQuery] double? maxGia)
        {
            var builder = Builders<BatDongSan>.Filter;
            var filter = builder.Empty;


            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var keywordFilter = builder.Or(
                    builder.Regex(x => x.TieuDe, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                    builder.Regex(x => x.DiaChi, new MongoDB.Bson.BsonRegularExpression(keyword, "i"))
                );
                filter &= keywordFilter;
            }

            if (!string.IsNullOrWhiteSpace(loaiHinh) && loaiHinh != "Tất cả")
            {
                filter &= builder.Eq(x => x.LoaiHinh, loaiHinh);
            }

            if (minGia.HasValue)
            {
                filter &= builder.Gte(x => x.Gia, minGia.Value);
            }
            if (maxGia.HasValue)
            {
                filter &= builder.Lte(x => x.Gia, maxGia.Value); 
            }

            var list = await _bdsCollection.Find(filter)
                .SortByDescending(x => x.Id)
                .ToListAsync();

            return Ok(list);
        }
    }
}