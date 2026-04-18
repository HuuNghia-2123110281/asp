using asp.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyImageController : ControllerBase
    {
        private readonly IMongoCollection<PropertyImage> _imageCollection;

        public PropertyImageController(IMongoDatabase database)
        {
            _imageCollection = database.GetCollection<PropertyImage>("PropertyImages");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PropertyImage>>> GetAll()
        {
            return Ok(await _imageCollection.Find(_ => true).ToListAsync());
        }

        [HttpGet("property/{propertyId}")]
        public async Task<ActionResult<IEnumerable<PropertyImage>>> GetImagesByPropertyId(string propertyId)
        {
            var images = await _imageCollection.Find(i => i.PropertyId == propertyId).ToListAsync();
            return Ok(images);
        }

        [HttpPost]
        public async Task<ActionResult> Create(PropertyImage image)
        {
            await _imageCollection.InsertOneAsync(image);
            return Ok(new { message = "Thêm hình ảnh thành công!", data = image });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var result = await _imageCollection.DeleteOneAsync(i => i.Id == id);
            if (result.DeletedCount == 0) return NotFound(new { message = "Không tìm thấy ảnh" });
            return Ok(new { message = "Xóa ảnh thành công!" });
        }
    }
}