using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SeoAuto.IdentityService.Infrastructure.Database;

namespace SeoAuto.IdentityService.Features.Auth.Login;

public record LoginUserRequest(string Email, string Password);
public record LoginUserResponse(string AccessToken, string RefreshToken, string TokenType, int ExpiresIn);

public static class LoginUserEndpoint
{
    public static void MapLoginUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (LoginUserRequest request, AppDbContext dbContext, IConfiguration config) =>
        {
            // 1. Kiểm tra đầu vào
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { Message = "Email và Mật khẩu không được để trống." });
            }

            // 2. Tìm User trong Database
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return Results.BadRequest(new { Message = "Thông tin đăng nhập không chính xác." });
            }

            // 3. Xung đối mật khẩu BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Results.BadRequest(new { Message = "Thông tin đăng nhập không chính xác." });
            }

            // 4. Sinh mã Access Token (JWT)
            var secretKey = config["JwtSettings:Secret"] ?? "SuperSecretKeyForSeoAutoV2ProjectMinimum32BytesLong!";
            var issuer = config["JwtSettings:Issuer"] ?? "SeoAutoV2";
            var audience = config["JwtSettings:Audience"] ?? "SeoAutoV2Clients";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("fullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expires = DateTime.UtcNow.AddMinutes(120);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.WriteToken(token);

            // 5. Sinh mã Refresh Token ngẫu nhiên (Hạn dùng 7 ngày)
            var refreshTokenBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(refreshTokenBytes);
            var refreshToken = Convert.ToBase64String(refreshTokenBytes);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new LoginUserResponse(accessToken, refreshToken, "Bearer", 120 * 60));
        })
        .WithName("LoginUser")
        .WithTags("Auth");
    }
}
