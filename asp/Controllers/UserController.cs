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

        // 1. LẤY DANH SÁCH TOÀN BỘ NGƯỜI DÙNG
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
        {
            var users = await _userCollection.Find(_ => true)
                .Project(u => new { u.Id, u.Username, u.FullName, u.Email, u.Phone, u.Role })
                .ToListAsync();
            return Ok(users);
        }

        // 2. LẤY THÔNG TIN HỒ SƠ
        [HttpGet("{username}")]
        public async Task<IActionResult> GetProfile(string username)
        {
            var user = await _userCollection.Find(u => u.Username == username).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin người dùng!" });
            }

            return Ok(new
            {
                username = user.Username,
                fullName = user.FullName ?? user.Username,
                email = user.Email,
                phone = user.Phone,
                role = user.Role
            });
        }

        // 3. THÊM TÀI KHOẢN MỚI
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User newUser)
        {
            var exists = await _userCollection.Find(u => u.Username == newUser.Username).FirstOrDefaultAsync();
            if (exists != null) return BadRequest(new { message = "Tên đăng nhập này đã tồn tại!" });

            await _userCollection.InsertOneAsync(newUser);
            return Ok(new { message = "Thêm tài khoản thành công bởi Nguyen Huu Nghia!", data = newUser });
        }

        // 4. CHỈNH SỬA HỒ SƠ
        [HttpPut("{username}")]
        public async Task<IActionResult> UpdateProfile(string username, [FromBody] User updatedInfo)
        {
            var user = await _userCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
            if (user == null) return NotFound(new { message = "Không tìm thấy thông tin người dùng!" });

            if (!string.IsNullOrEmpty(updatedInfo.FullName)) user.FullName = updatedInfo.FullName;
            if (!string.IsNullOrEmpty(updatedInfo.Email)) user.Email = updatedInfo.Email;
            if (!string.IsNullOrEmpty(updatedInfo.Phone)) user.Phone = updatedInfo.Phone;

            await _userCollection.ReplaceOneAsync(u => u.Username == username, user);

            return Ok(new { message = "Cập nhật hồ sơ thành công bởi Nguyen Huu Nghia!", fullName = user.FullName });
        }

        // 5. XÓA NGƯỜI DÙNG
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userCollection.DeleteOneAsync(u => u.Id == id);
            if (result.DeletedCount == 0) return NotFound(new { message = "Không tìm thấy nhân viên!" });

            return Ok(new { message = "Đã xóa nhân viên thành công bởi Nguyen Huu Nghia!" });
        }
    }
}