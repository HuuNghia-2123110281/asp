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

        // 1. THÊM MỚI: Upload file Hợp đồng đa định dạng
        [HttpPost("upload")]
        public async Task<IActionResult> UploadContract([FromForm] string transactionId, [FromForm] string contractNumber, IFormFile file)
        {
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Không tìm thấy file tải lên!" });

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Hệ thống chỉ chấp nhận định dạng PDF, Word (.doc, .docx) hoặc Hình ảnh (.jpg, .png)!" });

            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contracts");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var contract = new Contract
            {
                TransactionId = transactionId,
                ContractNumber = contractNumber,
                FileUrl = $"/uploads/contracts/{fileName}"
            };
            await _contractCollection.InsertOneAsync(contract);

            return Ok(new { message = "Lưu hợp đồng thành công!", data = contract });
        }

        // 2. XEM CHI TIẾT: Lấy hợp đồng ra xem theo Mã Giao Dịch
        [HttpGet("transaction/{transId}")]
        public async Task<IActionResult> GetByTransaction(string transId)
        {
            var contract = await _contractCollection.Find(c => c.TransactionId == transId).FirstOrDefaultAsync();
            if (contract == null) return NotFound();
            return Ok(contract);
        }

        // 3. SỬA: Cập nhật hợp đồng 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContract(string id, [FromForm] string transactionId, [FromForm] string contractNumber, IFormFile? file)
        {
            var existingContract = await _contractCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (existingContract == null) return NotFound(new { message = "Không tìm thấy hợp đồng để cập nhật!" });

            existingContract.TransactionId = transactionId;
            existingContract.ContractNumber = contractNumber;
            if (file != null && file.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Định dạng file không hợp lệ!" });

                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contracts");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
                if (!string.IsNullOrEmpty(existingContract.FileUrl))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingContract.FileUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                existingContract.FileUrl = $"/uploads/contracts/{fileName}";
            }

            await _contractCollection.ReplaceOneAsync(c => c.Id == id, existingContract);
            return Ok(new { message = "Cập nhật hợp đồng thành công!" });
        }

        // 4. XÓA: Xóa hợp đồng khỏi Database và Xóa file trong ổ cứng
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContract(string id)
        {
            var contract = await _contractCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (contract == null) return NotFound(new { message = "Không tìm thấy hợp đồng!" });
            if (!string.IsNullOrEmpty(contract.FileUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", contract.FileUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            var result = await _contractCollection.DeleteOneAsync(c => c.Id == id);

            if (result.DeletedCount > 0)
                return Ok(new { message = "Đã xóa hợp đồng thành công!" });

            return BadRequest(new { message = "Lỗi khi xóa hợp đồng!" });
        }
        // LẤY TẤT CẢ DANH SÁCH HỢP ĐỒNG (Bổ sung thêm)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contract>>> GetAllContracts()
        {
            var list = await _contractCollection.Find(_ => true).ToListAsync();
            return Ok(list);
        }
    }
}