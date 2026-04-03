using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using asp.Data; // Đảm bảo đúng namespace chứa file User.cs của bạn

namespace asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMongoCollection<User> _userCollection;

        public AuthController(IMongoDatabase database)
        {
            // Kết nối tới Collection "Users" trong MongoDB
            _userCollection = database.GetCollection<User>("Users");
        }

        // 1. API ĐĂNG KÝ (Register)
        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            // Kiểm tra xem Username đã tồn tại chưa
            var existingUser = await _userCollection.Find(u => u.Username == user.Username).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return BadRequest(new { message = "Tài khoản này đã có người sử dụng!" });
            }

            // Lưu người dùng mới vào MongoDB
            await _userCollection.InsertOneAsync(user);
            return Ok(new { message = "Đăng ký thành công!" });
        }

        // 2. API ĐĂNG NHẬP (Login)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginInfo)
        {
            var user = await _userCollection.Find(u =>
                u.Username == loginInfo.Username &&
                u.Password == loginInfo.Password).FirstOrDefaultAsync();

            if (user == null) return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu!" });

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                fullName = user.FullName ?? user.Username, // Nếu không có FullName thì lấy Username hiện lên
                username = user.Username
            });
        }
    }
}