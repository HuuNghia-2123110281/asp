using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using asp.Data;
using System.Net;
using System.Net.Mail;

namespace asp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMongoCollection<User> _userCollection;

        public AuthController(IMongoDatabase database)
        {
            _userCollection = database.GetCollection<User>("Users");
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            var existingUser = await _userCollection.Find(u => u.Username == user.Username).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return BadRequest(new
                {
                    message = "Tài khoản này đã có người sử dụng!",
                    developer = "Nguyen Huu Nghia"
                });
            }

            if (string.IsNullOrEmpty(user.Role))
            {
                user.Role = "KhachHang";
            }

            await _userCollection.InsertOneAsync(user);
            return Ok(new
            {
                message = "Đăng ký thành công!",
                developer = "Nguyen Huu Nghia"
            });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginInfo)
        {
            var user = await _userCollection.Find(u =>
                u.Username == loginInfo.Username &&
                u.Password == loginInfo.Password).FirstOrDefaultAsync();

            if (user == null) return Unauthorized(new
            {
                message = "Sai tài khoản hoặc mật khẩu!",
                developer = "Nguyen Huu Nghia"
            });

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                fullName = user.FullName ?? user.Username,
                username = user.Username,
                role = user.Role,
                developer = "Nguyen Huu Nghia"
            });
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _userCollection.Find(u => u.Username == request.Username).FirstOrDefaultAsync();

            if (user == null)
                return BadRequest(new { message = "Số điện thoại này chưa được đăng ký!", developer = "Nguyen Huu Nghia" });

            if (string.IsNullOrEmpty(user.Email))
                return BadRequest(new { message = "Tài khoản này chưa liên kết Email. Vui lòng liên hệ Admin!", developer = "Nguyen Huu Nghia" });


            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var newPassword = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());


            user.Password = newPassword;
            await _userCollection.ReplaceOneAsync(u => u.Id == user.Id, user);

            try
            {

                var fromAddress = new MailAddress("THAY-EMAIL-CỦA-BẠN-VÀO-ĐÂY@gmail.com", "BĐS Hỗ Trợ");
                var toAddress = new MailAddress(user.Email);


                const string fromPassword = "xxxxxxxxxxxxxxxx";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = "Khôi phục mật khẩu - BĐS",
                    Body = $@"
                        <h3>Chào {user.FullName ?? user.Username},</h3>
                        <p>Hệ thống vừa nhận được yêu cầu khôi phục mật khẩu cho tài khoản: <b>{user.Username}</b>.</p>
                        <p>Mật khẩu đăng nhập mới của bạn là: <strong style='color:red; font-size:18px;'>{newPassword}</strong></p>
                        <p>Vui lòng đăng nhập và đổi lại mật khẩu ngay để bảo mật tài khoản.</p>
                        <br>
                        <p>Trân trọng,</p>
                        <p><b>Hệ thống quản trị bởi Nguyen Huu Nghia</b></p>",
                    IsBodyHtml = true
                })
                {
                    await smtp.SendMailAsync(message);
                }

                return Ok(new
                {
                    message = "Mật khẩu mới đã được gửi tới Email của bạn!",
                    developer = "Nguyen Huu Nghia"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Lỗi khi cấu hình gửi Email. Vui lòng kiểm tra lại!",
                    developer = "Nguyen Huu Nghia"
                });
            }
        }
    }
    public class ForgotPasswordRequest
    {
        public string Username { get; set; }
    }
}