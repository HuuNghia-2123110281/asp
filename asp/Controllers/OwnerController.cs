using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using asp.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerController : ControllerBase
    {
        private readonly IMongoCollection<Owner> _ownerCollection;
        private readonly IMongoCollection<BatDongSan> _bdsCollection;

        public OwnerController(IMongoDatabase database)
        {
            _ownerCollection = database.GetCollection<Owner>("Owners");
            _bdsCollection = database.GetCollection<BatDongSan>("BatDongSans");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Owner>>> GetAll() => Ok(await _ownerCollection.Find(_ => true).ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create(Owner owner)
        {
            await _ownerCollection.InsertOneAsync(owner);
            return Ok(new { message = "Thêm chủ nhà thành công!", data = owner });
        }

        //Lấy danh sách BĐS thuộc sở hữu của Chủ nhà này
        [HttpGet("{id}/properties")]
        public async Task<IActionResult> GetPropertiesByOwner(string id)
        {
            var properties = await _bdsCollection.Find(p => p.OwnerId == id).ToListAsync();
            return Ok(properties);
        }
    }
}