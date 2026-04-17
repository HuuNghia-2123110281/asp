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

        // Kết nối vào collection "Customers" trong MongoDB
        public CustomerController(IMongoDatabase database)
        {
            _customerCollection = database.GetCollection<Customer>("Customers");
        }

        // 1. Lấy danh sách toàn bộ khách hàng
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            var list = await _customerCollection.Find(_ => true)
                .SortByDescending(x => x.Id)
                .ToListAsync();
            return Ok(list);
        }

        // 2. Thêm khách hàng mới (Pass Test Case: Bắt lỗi bỏ trống tên/sđt)
        [HttpPost]
        public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.Name) || string.IsNullOrEmpty(customer.Phone))
            {
                return BadRequest(new { message = "Tên và Số điện thoại là bắt buộc!" });
            }

            await _customerCollection.InsertOneAsync(customer);
            return Ok(customer);
        }
    }
}