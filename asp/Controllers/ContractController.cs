using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using asp.Data;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly IMongoCollection<Contract> _contractCollection;

        public ContractController(IMongoDatabase database)
        {
            _contractCollection = database.GetCollection<Contract>("Contracts");
        }

        // Upload file Hợp đồng đa định dạng
        [HttpPost("upload")]
        public async Task<IActionResult> UploadContract([FromForm] string transactionId, [FromForm] string contractNumber, IFormFile file)
        {
            // Danh sách các đuôi file được phép tải lên
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Không tìm thấy file tải lên!" });

            // Lấy đuôi file hiện tại và chuyển thành chữ thường để so sánh
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Hệ thống chỉ chấp nhận định dạng PDF, Word (.doc, .docx) hoặc Hình ảnh (.jpg, .png)!" });

            // Tạo thư mục nếu chưa có
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contracts");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            // Giữ lại đuôi file gốc khi lưu
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Lưu vào MongoDB
            var contract = new Contract
            {
                TransactionId = transactionId,
                ContractNumber = contractNumber,
                FileUrl = $"/uploads/contracts/{fileName}"
            };
            await _contractCollection.InsertOneAsync(contract);

            return Ok(new { message = "Lưu hợp đồng thành công!", data = contract });
        }

        // Lấy hợp đồng ra xem
        [HttpGet("transaction/{transId}")]
        public async Task<IActionResult> GetByTransaction(string transId)
        {
            var contract = await _contractCollection.Find(c => c.TransactionId == transId).FirstOrDefaultAsync();
            if (contract == null) return NotFound();
            return Ok(contract);
        }
    }
}