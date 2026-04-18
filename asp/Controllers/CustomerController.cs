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

        //Xem chi tiết 1 khách hàng
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetById(string id)
        {
            var customer = await _customerCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (customer == null) return NotFound();
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

        //Cập nhật thông tin khách hàng
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Customer updatedCustomer)
        {
            var result = await _customerCollection.ReplaceOneAsync(c => c.Id == id, updatedCustomer);
            if (result.MatchedCount == 0) return NotFound();
            return Ok(new { message = "Cập nhật thành công!" });
        }
    }
}