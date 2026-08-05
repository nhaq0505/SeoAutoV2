using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SeoAuto.IdentityService.Infrastructure.Database;

namespace SeoAuto.IdentityService.Features.Users.UpdateProfile;

public record UpdateProfileRequest(string? FullName, string? Email);
public record UpdateProfileResponse(Guid Id, string Email, string FullName);


public static class UpdateProfileEndpoint
{
    public static void MapUpdateProfileEndPoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/users/me",
                async (UpdateProfileRequest request, ClaimsPrincipal userClaims, AppDbContext dbContext) =>
                {
                    string? email = request.Email;
                    string? fullName = request.FullName;

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
                    
                    
                    if (!string.IsNullOrWhiteSpace(email) && email != user.Email)
                    {
                        // 2. Soi trong DB xem có ai KHÁC (u.Id != userId) đang dùng email này không
                        bool isEmailTaken = await dbContext.Users.AnyAsync(u => u.Email == email && u.Id != userId);
                        if (isEmailTaken)
                        {
                            return Results.Conflict(new { Message = "Email này đã được sử dụng bởi một tài khoản khác." });
                        }
                        // 3. Nếu không trùng thì mới gán Email mới vào
                        user.Email = email;
                    }
                    if (!string.IsNullOrWhiteSpace(fullName)){user.FullName = fullName;}
                    await dbContext.SaveChangesAsync();
                    return Results.Ok(new UpdateProfileResponse(user.Id, user.Email, user.FullName));




                })
            .RequireAuthorization() // 🔒 Khóa API này lại: Bắt buộc phải có JWT Token hợp lệ mới cho truy cập!
            .WithName("UpdateProfile")
            .WithTags("Users");

    }
}