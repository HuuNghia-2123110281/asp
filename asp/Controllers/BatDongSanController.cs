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

        public BatDongSanController(IMongoDatabase database)
        {
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

        // 3. THÊM MỚI BĐS - LƯU ẢNH VÀO DATABASE BẰNG BASE64
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
                    using (var memoryStream = new MemoryStream())
                    {
                        await request.HinhAnhFile.CopyToAsync(memoryStream);
                        byte[] imageBytes = memoryStream.ToArray();
                        string base64String = Convert.ToBase64String(imageBytes);

                        hinhAnhUrl = $"data:{request.HinhAnhFile.ContentType};base64,{base64String}";
                    }
                }

                var batDongSan = new BatDongSan
                {
                    TieuDe = request.TieuDe,
                    LoaiGiaoDich = request.LoaiGiaoDich,
                    LoaiHinh = request.LoaiHinh,
                    DienTich = request.DienTich,
                    PhongNgu = request.PhongNgu,
                    Gia = request.Gia,
                    DiaChi = request.DiaChi,
                    MoTa = request.MoTa,
                    ProjectId = request.ProjectId,
                    OwnerId = request.OwnerId,
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

        // 4. SỬA BĐS
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBatDongSan(string id, [FromForm] BatDongSanRequest request)
        {
            var existingBds = await _bdsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (existingBds == null) return NotFound();

            string? hinhAnhUrl = existingBds.HinhAnhUrl;

            if (request.HinhAnhFile != null && request.HinhAnhFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await request.HinhAnhFile.CopyToAsync(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();
                    string base64String = Convert.ToBase64String(imageBytes);
                    hinhAnhUrl = $"data:{request.HinhAnhFile.ContentType};base64,{base64String}";
                }
            }

            existingBds.TieuDe = request.TieuDe;
            existingBds.LoaiGiaoDich = request.LoaiGiaoDich;
            existingBds.LoaiHinh = request.LoaiHinh;
            existingBds.Gia = request.Gia;
            existingBds.DiaChi = request.DiaChi;
            existingBds.MoTa = request.MoTa;
            existingBds.DienTich = request.DienTich;
            existingBds.PhongNgu = request.PhongNgu;
            existingBds.ProjectId = request.ProjectId;
            existingBds.OwnerId = request.OwnerId;
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

        // 6. TÌM KIẾM CƠ BẢN
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<BatDongSan>>> SearchBatDongSan([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                var allList = await _bdsCollection.Find(_ => true).SortByDescending(x => x.Id).ToListAsync();
                return Ok(allList);
            }
            var filter = Builders<BatDongSan>.Filter.Or(
                Builders<BatDongSan>.Filter.Regex(x => x.TieuDe, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                Builders<BatDongSan>.Filter.Regex(x => x.DiaChi, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                Builders<BatDongSan>.Filter.Regex(x => x.LoaiHinh, new MongoDB.Bson.BsonRegularExpression(keyword, "i"))
            );
            var list = await _bdsCollection.Find(filter).SortByDescending(x => x.Id).ToListAsync();
            return Ok(list);
        }

        // 7. TÌM KIẾM NÂNG CAO (LỌC THEO TIÊU CHÍ)
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<BatDongSan>>> FilterBatDongSan(
            [FromQuery] string? keyword,
            [FromQuery] string? loaiHinh,
            [FromQuery] double? minGia,
            [FromQuery] double? maxGia,
            [FromQuery] double? minDienTich,
            [FromQuery] double? maxDienTich,
            [FromQuery] int? phongNgu,
            [FromQuery] string? loaiGiaoDich)
        {
            var builder = Builders<BatDongSan>.Filter;
            var filter = builder.Empty;

            // 1. Lọc theo từ khóa
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var keywordFilter = builder.Or(
                    builder.Regex(x => x.TieuDe, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                    builder.Regex(x => x.DiaChi, new MongoDB.Bson.BsonRegularExpression(keyword, "i"))
                );
                filter &= keywordFilter;
            }

            // 2. Lọc theo loại hình
            if (!string.IsNullOrWhiteSpace(loaiHinh) && loaiHinh != "Tất cả")
            {
                filter &= builder.Eq(x => x.LoaiHinh, loaiHinh);
            }

            // 3. Lọc theo giá
            if (minGia.HasValue)
            {
                filter &= builder.Gte(x => x.Gia, minGia.Value);
            }
            if (maxGia.HasValue)
            {
                filter &= builder.Lte(x => x.Gia, maxGia.Value);
            }

            // 4. LỌC THEO DIỆN TÍCH
            if (minDienTich.HasValue)
            {
                filter &= builder.Gte(x => x.DienTich, minDienTich.Value);
            }
            if (maxDienTich.HasValue)
            {
                filter &= builder.Lte(x => x.DienTich, maxDienTich.Value);
            }

            // 5. LỌC THEO PHÒNG NGỦ
            if (phongNgu.HasValue)
            {
                if (phongNgu.Value >= 5)
                {
                    filter &= builder.Gte(x => x.PhongNgu, 5);
                }
                else
                {
                    filter &= builder.Eq(x => x.PhongNgu, phongNgu.Value);
                }
            }

            // 6. BỔ SUNG: LỌC THEO LOẠI GIAO DỊCH (Bán / Cho thuê)
            if (!string.IsNullOrWhiteSpace(loaiGiaoDich))
            {
                filter &= builder.Eq(x => x.LoaiGiaoDich, loaiGiaoDich);
            }

            var list = await _bdsCollection.Find(filter)
                .SortByDescending(x => x.Id)
                .ToListAsync();

            return Ok(list);
        }
    }
}