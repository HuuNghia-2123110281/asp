using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using asp.Data;

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

        // Lấy danh sách khách hàng (Module 8.2)
        [HttpGet]
        public async Task<List<Customer>> Get() => await _customerCollection.Find(_ => true).ToListAsync();

        // Thêm khách hàng mới (Module 5.2.1)
        [HttpPost]
        public async Task<IActionResult> Post(Customer customer)
        {
            await _customerCollection.InsertOneAsync(customer);
            return Ok(customer);
        }
    }
}