using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using asp.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IMongoCollection<Customer> _customerCollection;

        public CustomerController(IMongoDatabase database)
        {
            _customerCollection = database.GetCollection<Customer>("Customers");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return Ok(await _customerCollection.Find(_ => true).SortByDescending(x => x.Id).ToListAsync());
        }

        // Xem chi tiết 1 khách hàng
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetById(string id)
        {
            var customer = await _customerCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (customer == null) return NotFound(new { message = "Không tìm thấy khách hàng!" });
            return Ok(customer);
        }

        [HttpPost]
        public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.Name) || string.IsNullOrEmpty(customer.Phone))
                return BadRequest(new { message = "Tên và Số điện thoại là bắt buộc!" });

            await _customerCollection.InsertOneAsync(customer);
            return Ok(customer);
        }

        // Cập nhật thông tin khách hàng
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Customer updatedCustomer)
        {
            var result = await _customerCollection.ReplaceOneAsync(c => c.Id == id, updatedCustomer);
            if (result.MatchedCount == 0) return NotFound(new { message = "Không tìm thấy khách hàng để cập nhật!" });
            return Ok(new { message = "Cập nhật thành công!" });
        }
        // --- CHỈ THÊM 2 HÀM NÀY VÀO CUSTOMER CONTROLLER ---

        // 1. SỬA THÔNG TIN KHÁCH HÀNG (PUT)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(string id, [FromBody] Customer customerIn)
        {
            try
            {
                var existing = await _customerCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
                if (existing == null) return NotFound(new { message = "Không tìm thấy khách hàng!" });

                // Cập nhật thông tin mới
                existing.FullName = customerIn.FullName;
                existing.Phone = customerIn.Phone;
                existing.Email = customerIn.Email;
                existing.Address = customerIn.Address;

                await _customerCollection.ReplaceOneAsync(c => c.Id == id, existing);
                return Ok(new { message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Server: " + ex.Message });
            }
        }

        // 2. XÓA KHÁCH HÀNG (DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            try
            {
                var result = await _customerCollection.DeleteOneAsync(c => c.Id == id);
                if (result.DeletedCount > 0)
                    return Ok(new { message = "Xóa khách hàng thành công!" });

                return NotFound(new { message = "Không tìm thấy khách hàng!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Server: " + ex.Message });
            }
        }
    }
}