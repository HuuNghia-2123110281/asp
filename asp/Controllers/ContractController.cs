using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using asp.Data;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System;

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

        // Upload file PDF Hợp đồng
        [HttpPost("upload")]
        public async Task<IActionResult> UploadContract([FromForm] string transactionId, [FromForm] string contractNumber, IFormFile file)
        {
            if (file == null || file.Length == 0 || file.ContentType != "application/pdf")
                return BadRequest(new { message = "Vui lòng chọn file định dạng PDF!" });

            // Tạo thư mục nếu chưa có
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contracts");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            // Lưu file với tên ngẫu nhiên để tránh trùng lặp
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
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