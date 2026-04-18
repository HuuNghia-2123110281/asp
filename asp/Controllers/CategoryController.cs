using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using asp.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMongoCollection<Category> _categoryCollection;

        public CategoryController(IMongoDatabase database)
        {
            _categoryCollection = database.GetCollection<Category>("Categories");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAll() => Ok(await _categoryCollection.Find(_ => true).ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            await _categoryCollection.InsertOneAsync(category);
            return Ok(new { message = "Thêm danh mục thành công!", data = category });
        }

        //Sửa danh mục
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Category updatedCat)
        {
            await _categoryCollection.ReplaceOneAsync(c => c.Id == id, updatedCat);
            return Ok(new { message = "Cập nhật danh mục thành công!" });
        }
    }
}