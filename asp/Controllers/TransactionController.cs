using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using asp.Data;

namespace asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IMongoCollection<Transaction> _transactionCollection;

        public TransactionController(IMongoDatabase database)
        {
            _transactionCollection = database.GetCollection<Transaction>("Transactions");
        }

        // Lấy danh sách giao dịch (Module 8.3)
        [HttpGet]
        public async Task<List<Transaction>> Get() => await _transactionCollection.Find(_ => true).ToListAsync();

        // Tạo giao dịch mới (Module 5.3.1)
        [HttpPost]
        public async Task<IActionResult> Post(Transaction transaction)
        {
            await _transactionCollection.InsertOneAsync(transaction);
            return Ok(transaction);
        }
    }
}