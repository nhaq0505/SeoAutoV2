using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SeoAuto.IdentityService.Infrastructure.Database;

namespace SeoAuto.IdentityService.Features.Auth;

public record RefreshTokenRequest(string RefreshToken);
public record RefreshTokenResponse(string AccessToken, string RefreshToken, string TokenType, int ExpiresIn);

public static class RefreshTokenEndpoint
{
    public static void MapRefreshTokenEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/refresh", async (RefreshTokenRequest request, AppDbContext dbContext, IConfiguration config) =>
        {
            // 1. Kiểm tra RefreshToken gửi lên
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return Results.BadRequest(new { Message = "Refresh token không được để trống." });
            }

            // 2. Tìm User sở hữu RefreshToken này trong DB
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);
            if (user == null)
            {
                return Results.BadRequest(new { Message = "Refresh token không hợp lệ." });
            }

            // 3. Kiểm tra hạn dùng của RefreshToken
            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Results.BadRequest(new { Message = "Refresh token đã hết hạn. Vui lòng đăng nhập lại." });
            }

            // 4. Sinh Access Token (JWT) mới
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
            var newAccessToken = tokenHandler.WriteToken(token);

            // 5. Sinh Refresh Token ngẫu nhiên mới (Refresh Token Rotation)
            var refreshTokenBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(refreshTokenBytes);
            var newRefreshToken = Convert.ToBase64String(refreshTokenBytes);

            // 6. Cập nhật Token mới vào DB
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await dbContext.SaveChangesAsync();

            // 7. Trả về cặp Token mới cho Client
            return Results.Ok(new RefreshTokenResponse(newAccessToken, newRefreshToken, "Bearer", 120 * 60));
        })
        .WithName("RefreshToken")
        .WithTags("Auth");
    }
}
