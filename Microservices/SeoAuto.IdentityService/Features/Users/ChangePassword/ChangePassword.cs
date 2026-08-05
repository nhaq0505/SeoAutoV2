using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SeoAuto.IdentityService.Infrastructure.Database;

namespace SeoAuto.IdentityService.Features.Users.ChangePassword;
public record ChangePasswordRequest(string OldPassword, string NewPassword);
public record ChangePasswordResponse(string Message);

public static class ChangePasswordEndpoint
{
    public static void MapChangePasswordEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/users/me/password",
            async (ChangePasswordRequest request, ClaimsPrincipal userClaims, AppDbContext dbContext) =>
            {
                string? oldPassword = request.OldPassword;
                string? newPassword = request.NewPassword;
                
                if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
                {
                    return Results.BadRequest(new { Message = "Vui lòng nhập đầy đủ mật khẩu cũ và mật khẩu mới." });
                }

                // 1. Trích xuất UserId từ mã JWT (Claims)
                var userIdClaim = userClaims.FindFirst(ClaimTypes.NameIdentifier)
                                  ?? userClaims.FindFirst("sub");

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Results.Unauthorized();
                }

                var user = await dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    return Results.NotFound("User not found");
                }

                bool isVerify = BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash);
                if (!isVerify)
                {
                    return Results.BadRequest("Old password is incorrect!");
                }
                
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);




                await dbContext.SaveChangesAsync();
                return Results.Ok(new ChangePasswordResponse("Change password successfully!"));




            })
            .RequireAuthorization()
            .WithName("ChangePassword")
            .WithTags("Users");
    }
}
