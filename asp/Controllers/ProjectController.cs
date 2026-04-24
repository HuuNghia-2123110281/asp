using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using asp.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IMongoCollection<Project> _projectCollection;

        public ProjectController(IMongoDatabase database)
        {
            _projectCollection = database.GetCollection<Project>("Projects");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetAll()
        {
            return Ok(await _projectCollection.Find(_ => true).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Project project)
        {
            await _projectCollection.InsertOneAsync(project);
            return Ok(new { message = "Thêm dự án thành công!", data = project });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(string id, [FromBody] Project projectIn)
        {
            try
            {
                var existing = await _projectCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
                if (existing == null) return NotFound(new { message = "Không tìm thấy dự án để cập nhật!" });

                projectIn.Id = existing.Id;
                await _projectCollection.ReplaceOneAsync(p => p.Id == id, projectIn);

                return Ok(new { message = "Cập nhật dự án thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Server: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(string id)
        {
            try
            {
                var result = await _projectCollection.DeleteOneAsync(p => p.Id == id);
                if (result.DeletedCount > 0)
                    return Ok(new { message = "Đã xóa dự án thành công!" });

                return NotFound(new { message = "Không tìm thấy dự án!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Server: " + ex.Message });
            }
        }
    }
}