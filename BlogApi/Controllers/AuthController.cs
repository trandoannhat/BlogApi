using BlogApi.DTOs;
using BlogApi.Helpers;
using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;

        public AuthController(IUnitOfWork uow, IConfiguration config)
        {
            _uow = uow;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            string action = "Register";
            var existingUser = await _uow.Users.Query().AnyAsync(u => u.Username == dto.Username);
            if (existingUser) return ErrorResponse(action, "Username đã tồn tại");

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _uow.Users.AddAsync(user);
            await _uow.CompleteAsync();

            return SuccessResponse<string>(null, action, "Đăng ký thành công");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            string action = "Login";
            var user = await _uow.Users.Query().FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new ApiResponse<string>(action, "Sai tài khoản hoặc mật khẩu"));

            var response = new TokenResponseDto
            {
                AccessToken = GenerateAccessToken(user),
                RefreshToken = GenerateRefreshToken()
            };

            user.RefreshToken = response.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _uow.CompleteAsync();

            return SuccessResponse(response, action, "Đăng nhập thành công");
        }
        private string GenerateAccessToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, "Admin") // Phân quyền Admin
    };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2), // Token sống trong 2 giờ
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenResponseDto dto)
        {
            string action = "RefreshToken";
            var user = await _uow.Users.Query()
                .FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized(new ApiResponse<string>(action, "Phiên đăng nhập hết hạn, vui lòng login lại"));

            // Cấp mới
            var response = new TokenResponseDto
            {
                AccessToken = GenerateAccessToken(user),
                RefreshToken = GenerateRefreshToken()
            };

            // Cập nhật lại vào DB (Rotate refresh token để bảo mật)
            user.RefreshToken = response.RefreshToken;
            await _uow.CompleteAsync();

            return SuccessResponse(response, action, "Lấy token mới thành công");
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Lấy Id người dùng từ Token đang sử dụng thông qua Claim
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _uow.Users.GetByIdAsync(int.Parse(userId));
            if (user != null)
            {
                user.RefreshToken = null; // Vô hiệu hóa refresh token
                await _uow.CompleteAsync();
            }

            return SuccessResponse<string>(null, "Logout", "Đã đăng xuất phía Server");
        }
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            // Nếu dữ liệu không hợp lệ (ví dụ mật khẩu quá ngắn), 
            // ASP.NET Core sẽ tự động trả về lỗi trước khi vào đến đây.

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _uow.Users.GetByIdAsync(userId);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
            {
                return ErrorResponse("ChangePassword", "Mật khẩu cũ không chính xác");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _uow.CompleteAsync();

            return SuccessResponse<string>(null, "ChangePassword", "Đổi mật khẩu thành công");
        }
    }

}
