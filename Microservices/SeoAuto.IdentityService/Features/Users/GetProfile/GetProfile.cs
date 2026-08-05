using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SeoAuto.IdentityService.Infrastructure.Database;

namespace SeoAuto.IdentityService.Features.Users.GetProfile;

// DTO trả về thông tin người dùng
public record UserProfileResponse(Guid Id, string Email, string FullName, string Role, DateTime CreatedAt);

public static class GetProfileEndpoint
{
    public static void MapGetProfileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users/me", async (ClaimsPrincipal userClaims, AppDbContext dbContext) =>
        {
            // 1. Trích xuất UserId từ mã JWT (Claims)
            var userIdClaim = userClaims.FindFirst(ClaimTypes.NameIdentifier) 
                           ?? userClaims.FindFirst("sub");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            // 2. Tìm thông tin User trong Database
            var user = await dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return Results.NotFound(new { Message = "Không tìm thấy thông tin người dùng." });
            }

            // 3. Trả về dữ liệu Profile
            var response = new UserProfileResponse(user.Id, user.Email, user.FullName, user.Role, user.CreatedAt);
            return Results.Ok(response);
        })
        .RequireAuthorization() // 🔒 Khóa API này lại: Bắt buộc phải có JWT Token hợp lệ mới cho truy cập!
        .WithName("GetUserProfile")
        .WithTags("Users");
    }
}
