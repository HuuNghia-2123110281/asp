using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using asp.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMongoCollection<User> _userCollection;

        public UserController(IMongoDatabase database)
        {
            _userCollection = database.GetCollection<User>("Users");
        }

        // Lấy danh sách toàn bộ nhân viên (Admin)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
        {

            var users = await _userCollection.Find(_ => true)
                .Project(u => new { u.Id, u.Username, u.FullName, u.Email, u.Role })
                .ToListAsync();
            return Ok(users);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userCollection.DeleteOneAsync(u => u.Id == id);
            if (result.DeletedCount == 0) return NotFound(new { message = "Không tìm thấy nhân viên" });
            return Ok(new { message = "Đã xóa nhân viên thành công" });
        }
    }
}